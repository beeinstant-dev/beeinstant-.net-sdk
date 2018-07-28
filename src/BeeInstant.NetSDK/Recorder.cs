using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BeeInstant.NetSDK.Abstractions;

namespace BeeInstant.NetSDK
{
    public class Recorder : IRecorder
    {
        private readonly ConcurrentQueue<double> queue;
        private readonly Unit _unit;

        public Recorder(Unit unit)
        {
            _unit = unit;
            queue = new ConcurrentQueue<double>();
        }

        public string FlushToString()
        {
            var values = new List<double>();

            while(queue.TryPeek(out double val))
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
            if(target == null)
            {
                return this;
            }

            var recorder = (Recorder)target;
            while(recorder.queue.TryPeek(out double res))
            {
                Record(res, recorder._unit);
            }

            return this;
        }

        public void Record(double value, Unit unit)
        {
            //TODO: implement equality comparer for Unit;
            if(_unit.Equals(unit))
            {
                queue.Enqueue(Math.Max(0.0d, value));
            }
        }
    }
}