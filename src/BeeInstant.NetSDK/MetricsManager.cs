using System;
using BeeInstant.NetSDK.Utils;
using System.Threading;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using BeeInstant.NetSDK.Http;
using System.Reactive.Linq;
using System.Collections.Concurrent;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Web;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Configuration;

namespace BeeInstant.NetSDK
{
    public class MetricsManager
    {
        private static volatile MetricsManager instance;
        private static object locker = new object();

        private static readonly string metricForErrors = "MetricErrors";
        private static readonly IDictionary<string, MetricsLogger> metricsLoggers = new Dictionary<string, MetricsLogger>();
        private static readonly ConcurrentQueue<string> metricsQueue = new ConcurrentQueue<string>();
        private static readonly ILoggerFactory _loggerFactory = new LoggerFactory();
        private static readonly MetricsManagerOptions _options = new MetricsManagerOptions();
        private static readonly ILogger _logger = _loggerFactory.AddConsole().CreateLogger(nameof(MetricsManager));
        
        private static string _serviceName;
        private static string _environment;
        private static string _hostInfo;
        private static MetricsLogger _rootMetricsLogger = null;
        private static HttpClient _httpClient;
        private static IDisposable _flushSchedulerHandler;

        private MetricsManager(string serviceName, string env, string hostInfo)
        {
            _serviceName = serviceName;
            _environment = env;
            _hostInfo = hostInfo;
        }

        public static MetricsManager Instance
        {
            get
            {
                return instance;
            }
        }

        public static void Initialize(string serviceName, string env, string hostInfo)
        {
            if (!Dimensions.IsValidName(serviceName))
            {
                throw new ArgumentException(nameof(serviceName));
            }

            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new MetricsManager(serviceName, env, hostInfo);

                        _environment = env;
                        _hostInfo = hostInfo;
                        _serviceName = serviceName;

                        var envDimension = GetEnvironmentDimension(env);

                        LoadOptionsFromConfigFile();

                        _rootMetricsLogger = GetOrAddRootMetricsLogger($"service={serviceName}{envDimension}");
                        _httpClient = ConfigureHttpClient();

                        if (!_options.IsManualFlush)
                        {
                            try
                            {
                                _flushSchedulerHandler = InitializeFlushScheduler();
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e.ToString());
                            }
                        }

                        _logger.LogInformation($"{nameof(MetricsManager)} successfuly initialized at {DateTime.UtcNow} UTC.");

                    }
                }
            }
        }

        public static void Initialize(string serviceName, string env)
        {
            string hostName = string.Empty;
            try
            {
                hostName = Dns.GetHostName();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw new InvalidOperationException("Unable to resolve the local host name.");
            }
            finally
            {
                Initialize(serviceName, env, hostName);
            }
        }

        public static void Initialize(string serviceName) => Initialize(serviceName, string.Empty);

        public static MetricsLogger GetMetricsLogger(string dimensions)
        {
            if (instance == null)
            {
                return new MetricsLogger("service=Dummy");
            }

            var dimensionsMap = Dimensions.ParseDimensions(dimensions);

            if (!dimensionsMap.Any())
            {
                throw new InvalidOperationException("Dimensions must be valid and non-empty");
            }

            dimensionsMap.AddOrUpdate("service", _serviceName);

            if (_environment.Length > 0)
            {
                dimensionsMap.AddOrUpdate("env", _environment);
            }

            var serializedDimensions = Dimensions.SerializeDimensionsToString(dimensionsMap);
            return metricsLoggers.GetOrAdd(serializedDimensions, new MetricsLogger(dimensionsMap));
        }

        public static MetricsLogger GetRootMetricsLogger() => _rootMetricsLogger ?? new MetricsLogger("service=Dummy");

        public static string GetEnvironment() => _environment;

        public static string GetServiceName() => _serviceName;

        public static string GetHostInfo() => _hostInfo;

        public static void FlushAll()
        {
            if (instance == null)
            {
                return;
            }

            var timeStamp = DateTimeHelpers.GetTimeStampInSeconds();

            foreach (var item in metricsLoggers.Values)
            {
                FlushMetricsLogger(item);
            }

            FlushToServer(timeStamp);
        }

        public static void Shutdown()
        {
            _flushSchedulerHandler?.Dispose();
            instance = null;
            _httpClient = null;
            _rootMetricsLogger = null;
            metricsLoggers.Clear();
            while(metricsQueue.TryDequeue(out string _));
        }

        public static void ReportError(string errorMessage)
        {
            if (instance != null)
            {
                _rootMetricsLogger?.IncrementCounter(metricForErrors, 1);
            }
            
            _logger.LogError(errorMessage);
        }

        internal static void FlushMetricsLogger(MetricsLogger logger)
        {
            logger.FlushToString((str) =>
            {
                metricsQueue.Enqueue(str);
            });

            _rootMetricsLogger.FlushToString((str) =>
            {
                metricsQueue.Enqueue(str);
            });
        }

        internal static void FlushToServer(long now)
        {
            _logger.LogDebug("Flush to BeeInstant Server");

            var readyToSubmit = new List<string>();
            readyToSubmit.AddRange(metricsQueue);

            var sb = new StringBuilder();

            foreach (var entry in readyToSubmit)
            {
                sb.AppendLine(entry);
            }

            if (readyToSubmit.Any())
            {
                try
                {
                    var body = sb.ToString();
                    var uri = new StringBuilder(Uri.EscapeUriString($"{_options.EndPoint}/PutMetric"));
                    var signature = Sign(body);

                    if (string.IsNullOrEmpty(signature))
                    {
                        return;
                    }

                    uri.Append("?signature=" + HttpUtility.UrlEncode(signature, Encoding.UTF8));
                    uri.Append("&publicKey=" + HttpUtility.UrlEncode(_options.PublicKey, Encoding.UTF8));
                    uri.Append("&timestamp=" + now);

                    var request = new HttpRequestMessage(HttpMethod.Post, uri.ToString());
                    // request.Properties.Add("content-type", "text/plain");
                    // request.Headers.Add("content-type", "text/plain");
                    request.Content = new StringContent(body, Encoding.UTF8, "text/plain");

                    var result = _httpClient.SendAsync(request).Result;
                    _logger.LogInformation($"Response: {result.Content.ReadAsStringAsync().Result}");
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                }
            }
        }

        private static void LoadOptionsFromConfigFile()
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("beeInstant.config.json", optional: false);

            IConfiguration config = builder.Build();

            config.Bind(_options);

            var sb = new StringBuilder();
            sb.AppendLine("beeInstant.config.json configuration: ");
            sb.AppendLine($"FlushInSeconds: {_options.FlushInSeconds}");
            sb.AppendLine($"FlushStartDelayInSeconds: {_options.FlushStartDelayInSeconds}");
            sb.AppendLine($"IsManualFlush: {_options.IsManualFlush}");
            sb.AppendLine($"PublicKey: {_options.PublicKey}");
            sb.AppendLine($"EndPoint: {_options.EndPoint}");

            _logger.LogInformation(sb.ToString());
        }

        private static string Sign(string entity)
        {
            if (!string.IsNullOrEmpty(_options.PublicKey) && !string.IsNullOrEmpty(_options.SecretKey))
            {
                var bytes = Encoding.UTF8.GetBytes(entity);
                try
                {
                    return Signature.Sign(bytes, _options.SecretKey);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                }
            }

            return String.Empty;
        }

        private static IDisposable InitializeFlushScheduler()
        {
            return Observable.Timer(TimeSpan.FromSeconds(_options.FlushStartDelayInSeconds), TimeSpan.FromSeconds(_options.FlushInSeconds))
                             .Subscribe(x =>
                             {
                                 FlushAll();
                             });
        }

        private static HttpClient ConfigureHttpClient()
        {
            var client = new HttpClient(new RetryHandler(new HttpClientHandler()));

            ServicePointManager.DefaultConnectionLimit = 2;
            client.DefaultRequestHeaders.Add("keep-alive", "60");

            return client;
        }

        private static string GetEnvironmentDimension(string env)
        {
            var envDimensions = String.Empty;

            if (env.Trim().Length > 0)
            {
                envDimensions = $",env={env.Trim()}";
            }

            return envDimensions;
        }

        private static MetricsLogger GetOrAddRootMetricsLogger(string name)
        {
            if (metricsLoggers.ContainsKey(name))
            {
                return metricsLoggers[name];
            }

            var logger = new MetricsLogger(name);

            metricsLoggers.Add(name, logger);

            return logger;
        }
    }
}