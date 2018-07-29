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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BeeInstant.NetSDK.Abstractions;
using BeeInstant.NetSDK.Utils;

namespace BeeInstant.NetSDK
{
    public class MetricsLogger : IMetricsComposer, IStringFlushableByAction
    {
        private readonly ConcurrentDictionary<string, MetricsCollector> _metricCollectors;
        private readonly IDictionary<string, string> _rootDimensions;
        private readonly MetricsGroup _rootMetricsGroup;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();


        public MetricsLogger() : this("")
        {
        }

        public MetricsLogger(string dimensions) : this(Dimensions.ParseDimensions(dimensions))
        {
        }

        public MetricsLogger(IDictionary<string, string> dimensionsMap)
        {
            _metricCollectors = new ConcurrentDictionary<string, MetricsCollector>();
            _rootDimensions = dimensionsMap;
            _rootMetricsGroup = new MetricsGroup(this, GetRootDimensionsString());
        }

        public virtual MetricsGroup ExtendDimensions(string dimensions) => new MetricsGroup(this, dimensions);

        public virtual MetricsGroup ExtendMultipleDimensions(params string[] dimensions) =>
            new MetricsGroup(this, dimensions);

        public virtual MetricsGroup ExtendMultipleDimensionsIncludeRoot(params string[] dimensions)
        {
            var dimensionGroupWithRoot = new string[dimensions.Length + 1];
            Array.Copy(dimensions, dimensionGroupWithRoot, dimensions.Length);
            dimensionGroupWithRoot[dimensions.Length] = GetRootDimensionsString();

            return new MetricsGroup(this, dimensionGroupWithRoot);
        }

        public virtual void Flush()
        {
            MetricsManager.FlushMetricsLogger(this);
            MetricsManager.FlushToServer(DateTimeHelpers.GetTimeStampInSeconds());
        }

        public virtual void IncrementCounter(string counterName, int value) =>
            _rootMetricsGroup.IncrementCounter(counterName, value);

        public virtual void Record(string metricName, decimal value, Unit unit) =>
            _rootMetricsGroup.Record(metricName, value, unit);

        public virtual TimerMetric StartTimer(string timerName) => _rootMetricsGroup.StartTimer(timerName);

        public virtual void StopTimer(string timerName) => _rootMetricsGroup.StopTimer(timerName);

        public virtual IDictionary<string, string> GetRootDimensions() =>
            new Dictionary<string, string>(_rootDimensions);

        public virtual void UpdateMetricsCollector(string dimensions, Action<MetricsCollector> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var metricsCollector = _metricCollectors.GetOrAdd(dimensions, new MetricsCollector());

            action.Invoke(metricsCollector);

            if (_metricCollectors.TryGetValue(dimensions, out var value) && metricsCollector != value)
            {
                AddOrMergeMetricsCollector(dimensions, metricsCollector);
            }
        }

        public virtual string FlushToString() => FlushToString(null);

        public virtual string FlushToString(Action<string> action)
        {
            Dictionary<string, MetricsCollector> readyToFlush;

            _locker.EnterWriteLock();

            try
            {
                readyToFlush = new Dictionary<string, MetricsCollector>(_metricCollectors);
                _metricCollectors.Clear();
            }
            finally
            {
                _locker.ExitWriteLock();
            }

            var sb = new StringBuilder();

            foreach (var toFlush in readyToFlush)
            {
                var metricsString = toFlush.Value.FlushToString();

                if (string.IsNullOrEmpty(metricsString))
                {
                    continue;
                }

                var metric = $"{toFlush.Key},{metricsString}";

                sb.AppendLine(metric);

                action?.Invoke(metric);
            }

            return sb.ToString();
        }

        public string GetRootDimensionsString()
        {
            var dimensions = _rootDimensions.Select(x => $"{x.Key}={x.Value}");
            return string.Join(",", dimensions);
        }

        private void AddOrMergeMetricsCollector(string dimensions, MetricsCollector collector)
        {
            _locker.EnterReadLock();
            try
            {
                if (_metricCollectors.TryAdd(dimensions, collector))
                {
                    _metricCollectors[dimensions].Merge(collector);
                }
            }
            finally
            {
                _locker.ExitReadLock();
            }
        }
    }
}