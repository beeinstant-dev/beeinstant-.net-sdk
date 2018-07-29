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
            {
                return;
            }

            Interlocked.Add(ref _count, value);
        }

        public ICounter Merge(ICounter target)
        {
            if (target == null || target == this)
            {
                return this;
            }

            var targetValue = target.GetValue();
            target.Reset();

            if (targetValue >= 0)
            {
                IncrementCounter(targetValue);
            }

            return this;
        }

        public long GetValue()
        {
            return Interlocked.Read(ref _count);
        }

        IMetric IMetric.Merge(IMetric target)
        {
            if (target is ICounter counter)
            {
                Merge(counter);
            }

            return this;
        }
    }
}