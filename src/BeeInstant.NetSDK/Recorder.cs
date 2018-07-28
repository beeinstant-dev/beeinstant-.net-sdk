using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BeeInstant.NetSDK.Abstractions;

namespace BeeInstant.NetSDK
{
    public class Recorder : IRecorder
    {
        private readonly ConcurrentQueue<decimal> queue;
        private readonly Unit _unit;

        public Recorder(Unit unit)
        {
            _unit = unit;
            queue = new ConcurrentQueue<decimal>();
        }

        public string FlushToString()
        {
            var values = new List<decimal>();

            while(queue.Any() && queue.TryDequeue(out decimal val))
            {
                values.Add(val);
            }

            if(values.Any())
            {
                return string.Join("+", values) + _unit;
            }

            return string.Empty;
        }

        public IRecorder Merge(IRecorder target)
        {
            if(target == null || target == this)
            {
                return this;
            }

            var recorder = (Recorder)target;
            while(recorder.queue.Any() && recorder.queue.TryDequeue(out decimal res))
            {
                Record(res, recorder._unit);
            }

            return this;
        }

        public void Record(decimal value, Unit unit)
        {
            if(_unit.Equals(unit))
            {
                queue.Enqueue(Math.Max(0.0M, value));
            }
        }

        IMetric IMetric.Merge(IMetric target)
        {
            if(target is IRecorder)
            {
                this.Merge(target as IRecorder);
            }

            return this;
        }
    }
}