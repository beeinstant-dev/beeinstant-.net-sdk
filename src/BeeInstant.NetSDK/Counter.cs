using System;
using System.Threading;
using BeeInstant.NetSDK.Abstractions;

namespace BeeInstant.NetSDK
{
    public class Counter : ICounter
    {
        private long _initialValue;
        private long _count;

        public Counter() => _count = _initialValue = 0;

        public Counter(long initialValue) => _count = _initialValue = initialValue;

        public string FlushToString()
        {
            var currentValue = Interlocked.Read(ref _count);

            this.Reset();

            return currentValue.ToString();
        }

        public void Reset()
        {
            Interlocked.Exchange(ref _count, Interlocked.Read(ref _initialValue));
        }

        public void IncrementCounter(long value)
        {
            if (value < 0)
                return;

            Interlocked.Add(ref _count, value);
        }

        public ICounter Merge(ICounter target)
        {
            if(target == null || target == this)
            {
                return this;
            }

            var targetValue = target.GetValue();
            target.Reset();

            if (targetValue >= 0)
            {
                this.IncrementCounter(targetValue);
            }

            return this;
        }

        public long GetValue()
        {
            return Interlocked.Read(ref _count);
        }

        IMetric IMetric.Merge(IMetric target)
        {
            if(target is ICounter)
            {
                this.Merge(target as ICounter);
            }

            return this;
        }
    }
}