using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BeeInstant.NetSDK.Abstractions;
using BeeInstant.NetSDK.Utils;

namespace BeeInstant.NetSDK
{
    public class MetricsCollector : IMetricsComposer<MetricsCollector>
    {
        private ConcurrentDictionary<string, IMetric> _metrics = new ConcurrentDictionary<string, IMetric>();

        public string FlushToString()
        {
            var flushedMetrics = new List<string>();

            foreach(var kv in _metrics)
            {
                var data = kv.Value.FlushToString();

                if(string.IsNullOrEmpty(data))
                {
                    continue;
                }

                flushedMetrics.Add($"m.{kv.Key}={data}");
            }

            return string.Join(",", flushedMetrics);
        }

        public void IncrementCounter(string counterName, int value)
        {
            if(!Dimensions.IsValidName(counterName))
            {
                //TODO: MetricsManager.reportError
                return;
            }

            if(_metrics.ContainsKey(counterName))
            {
                if(_metrics.TryGetValue(counterName, out IMetric cntr)
                    && cntr is ICounter)
                {
                    (cntr as ICounter).IncrementCounter(value);
                }
                
                return;
            }

            var counter = new Counter();
            counter.IncrementCounter(value);
            _metrics.TryAdd(counterName, counter);
        }

        public MetricsCollector Merge(MetricsCollector target)
        {
            if(target != this)
            {
                foreach(var targetMetric in target._metrics)
                {
                    _metrics.AddOrUpdate(targetMetric.Key, targetMetric.Value, (key, oldValue) => 
                    {
                        return targetMetric.Value.Merge(oldValue);
                    });
                }
            }

            return this;
        }

        IMetric IMetric.Merge(IMetric target)
        {
            if(target is MetricsCollector)
            {
                this.Merge(target as MetricsCollector);
            }

            return this;
        }

        public void Record(string metricName, decimal value, Unit unit)
        {
            if(!Dimensions.IsValidName(metricName))
            {
                return;
                //TODO: MetricsManager.reportError
                // throw new ArgumentException($"Invalid recorder name: {metricName}.");
            }

            if(_metrics.ContainsKey(metricName))
            {
                if(_metrics.TryGetValue(metricName, out IMetric rec) && rec is IRecorder)
                {
                    (rec as IRecorder).Record(value, unit);
                }
                
                return;
            }

            var recorder = new Recorder(unit);
            recorder.Record(value, unit);

            _metrics.TryAdd(metricName, recorder);
        }

        public TimerMetric StartTimer(string timerName)
        {
            if(!Dimensions.IsValidName(timerName))
            {
                //TODO: MetricsManager.reportError
                return null;
            }
            
            if(_metrics.ContainsKey(timerName))
            {
                if(_metrics.TryGetValue(timerName, out IMetric tim) && tim is ITimer)
                {
                    (tim as ITimer).StartTimer();
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
            if(!Dimensions.IsValidName(timerName))
            {
                //TODO: MetricsManager.reportError
                return;
            }

            if(_metrics.ContainsKey(timerName))
            {
                if(_metrics.TryGetValue(timerName, out IMetric tim) && tim is ITimer)
                {
                    (tim as ITimer).StopTimer();
                }
            }
        }

        public Dictionary<string, IMetric> GetMetrics()
        {
            return _metrics.ToDictionary(k => k.Key, v => v.Value);
        }
    }
}