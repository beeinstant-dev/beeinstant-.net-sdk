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

using System.Threading.Tasks;
using Xunit;

namespace BeeInstant.NetSDK.Tests
{
    public class CounterTests
    {
        [Fact]
        public void CounterIncrementIsThreadSafe()
        {
            var counter = new Counter();

            Parallel.For(0, 10000, (x) => { counter.IncrementCounter(1); });

            Assert.Equal(10000, counter.GetValue());
        }

        [Fact]
        public void FlushedCounterResetsToInitialValue()
        {
            var counter = new Counter(100);
            counter.IncrementCounter(1);

            var before = counter.FlushToString();
            var after = counter.FlushToString();

            Assert.Equal("101", before);
            Assert.Equal("100", after);
        }

        [Fact]
        public void ResetCounterSwitchesToInitialValue()
        {
            var counter = new Counter(120);
            var before = counter.GetValue();

            counter.IncrementCounter(5);
            counter.Reset();

            var after = counter.GetValue();

            Assert.Equal(before, after);
        }
    }
}