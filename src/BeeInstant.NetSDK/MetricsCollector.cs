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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BeeInstant.NetSDK.Abstractions;
using BeeInstant.NetSDK.Utils;

namespace BeeInstant.NetSDK
{
    public class MetricsCollector : IMetricsComposer<MetricsCollector>
    {
        private readonly ConcurrentDictionary<string, IMetric> _metrics = new ConcurrentDictionary<string, IMetric>();

        public string FlushToString()
        {
            var flushedMetrics = new List<string>();

            foreach (var kv in _metrics)
            {
                var data = kv.Value.FlushToString();

                if (string.IsNullOrEmpty(data))
                {
                    continue;
                }

                flushedMetrics.Add($"m.{kv.Key}={data}");
            }

            return string.Join(",", flushedMetrics);
        }

        public void IncrementCounter(string counterName, int value)
        {
            if (!Dimensions.IsValidName(counterName))
            {
                MetricsManager.ReportError($"Invalid counter name: {counterName}");
                return;
            }

            if (_metrics.ContainsKey(counterName))
            {
                if (_metrics.TryGetValue(counterName, out var mtrc) && mtrc is ICounter cntr)
                {
                    cntr.IncrementCounter(value);
                }

                return;
            }

            var counter = new Counter();
            counter.IncrementCounter(value);
            _metrics.TryAdd(counterName, counter);
        }

        public MetricsCollector Merge(MetricsCollector target)
        {
            if (target == this)
            {
                return this;
            }

            foreach (var targetMetric in target._metrics)
            {
                _metrics.AddOrUpdate(targetMetric.Key, targetMetric.Value,
                    (key, oldValue) => targetMetric.Value.Merge(oldValue));
            }

            return this;
        }

        IMetric IMetric.Merge(IMetric target)
        {
            if (target is MetricsCollector collector)
            {
                Merge(collector);
            }

            return this;
        }

        public void Record(string metricName, decimal value, Unit unit)
        {
            if (!Dimensions.IsValidName(metricName))
            {
                MetricsManager.ReportError($"Invalid metric name: {metricName}");
                return;
            }

            if (_metrics.ContainsKey(metricName))
            {
                if (_metrics.TryGetValue(metricName, out var metric) && metric is IRecorder rec)
                {
                    rec.Record(value, unit);
                }

                return;
            }

            var recorder = new Recorder(unit);
            recorder.Record(value, unit);

            _metrics.TryAdd(metricName, recorder);
        }

        public TimerMetric StartTimer(string timerName)
        {
            if (!Dimensions.IsValidName(timerName))
            {
                MetricsManager.ReportError($"Invalid timer name {timerName}");
                return null;
            }

            if (_metrics.ContainsKey(timerName))
            {
                if (_metrics.TryGetValue(timerName, out var metric) && metric is ITimer tim)
                {
                    tim.StartTimer();
                    return new TimerMetric(this, timerName);
                }
            }

            var timer = new Timer();
            timer.StartTimer();
            _metrics.TryAdd(timerName, timer);

            return new TimerMetric(this, timerName);
        }

        public void StopTimer(string timerName)
        {
            if (!Dimensions.IsValidName(timerName))
            {
                MetricsManager.ReportError($"Invalid timer name {timerName}");
                return;
            }

            if (!_metrics.ContainsKey(timerName))
            {
                return;
            }

            if (_metrics.TryGetValue(timerName, out var metric) && metric is ITimer tim)
            {
                tim.StopTimer();
            }
        }

        public Dictionary<string, IMetric> GetMetrics() => _metrics.ToDictionary(k => k.Key, v => v.Value);
    }
}