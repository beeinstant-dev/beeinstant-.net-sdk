using System;
using System.Collections.Generic;
using System.Linq;
using BeeInstant.NetSDK.Abstractions;
using BeeInstant.NetSDK.Utils;

namespace BeeInstant.NetSDK
{
    public class MetricsGroup : IMetricsComposer
    {
        private MetricsLogger _metricsLogger;
        private ICollection<string> _dimensionGroups;

        public MetricsGroup(MetricsLogger metricsLogger, params string[] dimensionGroups)
        {
            _metricsLogger = metricsLogger;
            _dimensionGroups = dimensionGroups.Select(x => Dimensions.ExtendAndSerializeDimensions(metricsLogger.GetRootDimensions(), x))
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