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
using System.Collections.Generic;
using System.Linq;
using BeeInstant.NetSDK.Abstractions;
using BeeInstant.NetSDK.Utils;

namespace BeeInstant.NetSDK
{
    public class MetricsGroup : IMetricsComposer
    {
        private readonly MetricsLogger _metricsLogger;
        private readonly ICollection<string> _dimensionGroups;

        public MetricsGroup(MetricsLogger metricsLogger, params string[] dimensionGroups)
        {
            _metricsLogger = metricsLogger;
            _dimensionGroups = dimensionGroups.Select(x =>
                    Dimensions.ExtendAndSerializeDimensions(metricsLogger.GetRootDimensions(), x))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
        }

        public virtual void IncrementCounter(string counterName, int value)
        {
            UpdateMetricsCollector(x => x.IncrementCounter(counterName, value));
        }

        public virtual void Record(string metricName, decimal value, Unit unit)
        {
            UpdateMetricsCollector(x => x.Record(metricName, value, unit));
        }

        public virtual TimerMetric StartTimer(string timerName)
        {
            UpdateMetricsCollector(x => x.StartTimer(timerName));
            return new TimerMetric(this, timerName);
        }

        public virtual void StopTimer(string timerName)
        {
            UpdateMetricsCollector(x => x.StopTimer(timerName));
        }

        private void UpdateMetricsCollector(Action<MetricsCollector> action)
        {
            foreach (var dim in _dimensionGroups)
            {
                _metricsLogger.UpdateMetricsCollector(dim, action);
            }
        }
    }
}