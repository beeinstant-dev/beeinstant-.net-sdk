/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2018 BeeInstant
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
 * documentation files (the "Software"), to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
 * to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions
 * of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
 */

using System;
using BeeInstant.NetSDK.Utils;
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

namespace BeeInstant.NetSDK
{
    public class MetricsManager
    {
        private static volatile MetricsManager _instance;
        private static readonly object Locker = new object();

        private const string MetricForErrors = "MetricErrors";

        private static readonly IDictionary<string, MetricsLogger> MetricsLoggers =
            new Dictionary<string, MetricsLogger>();

        private static readonly ConcurrentQueue<string> MetricsQueue = new ConcurrentQueue<string>();
        private static readonly ILoggerFactory LoggerFactory = new LoggerFactory();
        private static readonly MetricsManagerOptions Options = new MetricsManagerOptions();
        private static readonly ILogger Logger = LoggerFactory.AddConsole().CreateLogger(nameof(MetricsManager));

        private static string _serviceName;
        private static string _environment;
        private static string _hostInfo;
        private static MetricsLogger _rootMetricsLogger;
        private static HttpClient _httpClient;
        private static IDisposable _flushSchedulerHandler;

        private MetricsManager(string serviceName, string env, string hostInfo)
        {
            _serviceName = serviceName;
            _environment = env;
            _hostInfo = hostInfo;
        }

        public static void Initialize(string serviceName, string env, string hostInfo)
        {
            if (!Dimensions.IsValidName(serviceName))
            {
                throw new ArgumentException(nameof(serviceName));
            }

            if (_instance != null)
            {
                return;
            }

            lock (Locker)
            {
                if (_instance != null)
                {
                    return;
                }

                _instance = new MetricsManager(serviceName, env, hostInfo);

                _environment = env;
                _hostInfo = hostInfo;
                _serviceName = serviceName;

                var envDimension = GetEnvironmentDimension(env);

                LoadOptionsFromConfigFile();

                _rootMetricsLogger = GetOrAddRootMetricsLogger($"service={serviceName}{envDimension}");
                _httpClient = ConfigureHttpClient();

                if (!Options.IsManualFlush)
                {
                    try
                    {
                        _flushSchedulerHandler = InitializeFlushScheduler();
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e.ToString());
                    }
                }

                Logger.LogInformation($"{nameof(MetricsManager)} successfuly initialized at {DateTime.UtcNow} UTC.");
            }
        }

        public static void Initialize(string serviceName, string env)
        {
            var hostName = string.Empty;

            try
            {
                hostName = Dns.GetHostName();
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
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
            if (_instance == null)
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
            return MetricsLoggers.GetOrAdd(serializedDimensions, new MetricsLogger(dimensionsMap));
        }

        public static MetricsLogger GetRootMetricsLogger() => _rootMetricsLogger ?? new MetricsLogger("service=Dummy");

        public static string GetEnvironment() => _environment ?? string.Empty;

        public static string GetServiceName() => _serviceName ?? string.Empty;

        public static string GetHostInfo() => _hostInfo ?? string.Empty;

        public static void FlushAll()
        {
            if (_instance == null)
            {
                return;
            }

            var timeStamp = DateTimeHelpers.GetTimeStampInSeconds();

            foreach (var item in MetricsLoggers.Values)
            {
                FlushMetricsLogger(item);
            }

            FlushToServer(timeStamp);
        }

        public static void Shutdown()
        {
            _flushSchedulerHandler?.Dispose();

            _instance = null;
            _httpClient = null;
            _rootMetricsLogger = null;

            MetricsLoggers.Clear();

            while (MetricsQueue.TryDequeue(out _))
            {
            }
        }

        public static void ReportError(string errorMessage)
        {
            if (_instance != null)
            {
                _rootMetricsLogger?.IncrementCounter(MetricForErrors, 1);
            }

            Logger.LogError(errorMessage);
        }

        internal static void FlushMetricsLogger(MetricsLogger logger)
        {
            logger.FlushToString((str) => { MetricsQueue.Enqueue(str); });

            _rootMetricsLogger?.FlushToString((str) => { MetricsQueue.Enqueue(str); });
        }

        internal static void FlushToServer(long now)
        {
            Logger.LogDebug("Flush to BeeInstant Server");

            var readyToSubmit = new List<string>();
            readyToSubmit.AddRange(MetricsQueue);

            var sb = new StringBuilder();

            foreach (var entry in readyToSubmit)
            {
                sb.AppendLine(entry);
            }

            if (!readyToSubmit.Any())
            {
                return;
            }

            try
            {
                var body = sb.ToString();
                var uri = new StringBuilder(Uri.EscapeUriString($"{Options.EndPoint}/PutMetric"));
                var signature = Sign(body);

                if (string.IsNullOrEmpty(signature))
                {
                    return;
                }

                uri.Append("?signature=" + HttpUtility.UrlEncode(signature, Encoding.UTF8));
                uri.Append("&publicKey=" + HttpUtility.UrlEncode(Options.PublicKey, Encoding.UTF8));
                uri.Append("&timestamp=" + now);

                var request = new HttpRequestMessage(HttpMethod.Post, uri.ToString())
                {
                    Content = new StringContent(body, Encoding.UTF8, "text/plain")
                };

                var result = _httpClient.SendAsync(request).Result;
                var content = result.Content.ReadAsStringAsync().Result;
                var statusCode = result.StatusCode;
                Logger.LogInformation($"Response with status code {statusCode} and content: {content}");
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
        }

        private static void LoadOptionsFromConfigFile()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("beeInstant.config.json", optional: false);

            IConfiguration config = builder.Build();

            config.Bind(Options);

            var sb = new StringBuilder();
            sb.AppendLine("beeInstant.config.json configuration: ");
            sb.AppendLine($"FlushInSeconds: {Options.FlushInSeconds}");
            sb.AppendLine($"FlushStartDelayInSeconds: {Options.FlushStartDelayInSeconds}");
            sb.AppendLine($"IsManualFlush: {Options.IsManualFlush}");
            sb.AppendLine($"PublicKey: {Options.PublicKey}");
            sb.AppendLine($"EndPoint: {Options.EndPoint}");

            Logger.LogInformation(sb.ToString());
        }

        private static string Sign(string entity)
        {
            if (string.IsNullOrEmpty(Options.PublicKey) || string.IsNullOrEmpty(Options.SecretKey))
            {
                return string.Empty;
            }

            var bytes = Encoding.UTF8.GetBytes(entity);

            try
            {
                return Signature.Sign(bytes, Options.SecretKey);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }

            return string.Empty;
        }

        private static IDisposable InitializeFlushScheduler()
        {
            return Observable.Timer(TimeSpan.FromSeconds(Options.FlushStartDelayInSeconds),
                    TimeSpan.FromSeconds(Options.FlushInSeconds))
                .Subscribe(x => { FlushAll(); });
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
            var envDimensions = string.Empty;

            if (env.Trim().Length > 0)
            {
                envDimensions = $",env={env.Trim()}";
            }

            return envDimensions;
        }

        private static MetricsLogger GetOrAddRootMetricsLogger(string name)
        {
            if (MetricsLoggers.ContainsKey(name))
            {
                return MetricsLoggers[name];
            }

            var logger = new MetricsLogger(name);

            MetricsLoggers.Add(name, logger);

            return logger;
        }
    }
}