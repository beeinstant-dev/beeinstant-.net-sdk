using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BeeInstant.NetSDK.Abstractions;
using BeeInstant.NetSDK.Utils;

namespace BeeInstant.NetSDK
{
    public class MetricsLogger : IMetricsComposer, IStringFlushable
    {
        private readonly ConcurrentDictionary<string, MetricsCollector> _metricCollectors;
        private readonly IDictionary<string, string> _rootDimensions;
        private readonly MetricsGroup _rootMetricsGroup;
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        

        public MetricsLogger() : this("")
        {
        }

        public MetricsLogger(string dimensions) 
            : this(Dimensions.ParseDimensions(dimensions))
        {
        }

        public MetricsLogger(IDictionary<string, string> dimensionsMap)
        {
            _metricCollectors = new ConcurrentDictionary<string, MetricsCollector>();
            _rootDimensions = dimensionsMap;
            _rootMetricsGroup = new MetricsGroup(this, GetRootDimensionsString());
        }

        public MetricsGroup ExtendDimensions(string dimensions) => new MetricsGroup(this, dimensions);

        public MetricsGroup ExtendMultipleDimensions(params string[] dimensions) => new MetricsGroup(this, dimensions);

        public MetricsGroup ExtendMultipleDimensionsIncludeRoot(params string[] dimensions)
        {
            var dimensionGroupWithRoot = new string[dimensions.Length + 1];
            Array.Copy(dimensions, dimensionGroupWithRoot, dimensions.Length);
            dimensionGroupWithRoot[dimensions.Length] = GetRootDimensionsString();

            return new MetricsGroup(this, dimensionGroupWithRoot);
        }
        
        public void Flush(long now)
        {
            //TODO: implement
        }

        public void IncrementCounter(string counterName, int value) => _rootMetricsGroup.IncrementCounter(counterName, value);

        public void Record(string metricName, decimal value, Unit unit) => _rootMetricsGroup.Record(metricName, value, unit);

        public TimerMetric StartTimer(string timerName) => _rootMetricsGroup.StartTimer(timerName);

        public void StopTimer(string timerName) => _rootMetricsGroup.StopTimer(timerName);

        public IDictionary<string, string> GetRootDimensions() =>  new Dictionary<string, string>(_rootDimensions);
        
        public void UpdateMetricsCollector(string dimensions, Action<MetricsCollector> action)
        {
            if(action == null)
            {
                throw new ArgumentNullException(nameof(action));    
            }

            var metricsCollector = _metricCollectors.GetOrAdd(dimensions, new MetricsCollector());
            
            action.Invoke(metricsCollector);

            if(_metricCollectors.TryGetValue(dimensions, out MetricsCollector value) && metricsCollector != value)
            {
                AddOrMergeMetricsCollector(dimensions, metricsCollector);
            }
        }

        public string GetRootDimensionsString()
        {
            var dimensions = _rootDimensions.Select(x => $"{x.Key}={x.Value}");
            return string.Join(",", dimensions);
        }

        public string FlushToString()
        {
            Dictionary<string, MetricsCollector> readyToFlush;

            locker.EnterWriteLock();

            try
            {
                readyToFlush = new Dictionary<string, MetricsCollector>(_metricCollectors);
                _metricCollectors.Clear();
            }
            finally
            {
                locker.ExitWriteLock();
            }
            
            if(readyToFlush == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach(var toFlush in readyToFlush)
            {
                var metricsString = toFlush.Value.FlushToString();

                if(string.IsNullOrEmpty(metricsString))
                {
                    continue;
                }

                sb.AppendLine($"{toFlush.Key},{metricsString}");
            }

            return sb.ToString();
        }

        private void AddOrMergeMetricsCollector(string dimensions, MetricsCollector collector)
        {
            locker.EnterReadLock();
            try
            {
                if(_metricCollectors.TryAdd(dimensions, collector))
                {
                    _metricCollectors[dimensions].Merge(collector);
                }
            }
            finally
            {
                locker.ExitReadLock();
            }
        }
    }
}