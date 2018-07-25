using System;
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
            
            Parallel.For(0, 10000, (x) => {
                counter.IncrementCounter(1);
            });

            Assert.Equal(10000, counter.GetValue());
        }

        [Fact]
        public void FlushedCounterResetsToInitialValue()
        {
            var counter = new Counter(initialValue: 100);
            counter.IncrementCounter(1); 

            var before = counter.FlushToString();
            var after = counter.FlushToString();

            Assert.Equal("101", before);
            Assert.Equal("100", after);
        }

        [Fact]
        public void ResetCounterSwitchesToInitialValue()
        {
            var counter = new Counter(initialValue: 120);
            var before = counter.GetValue();

            counter.IncrementCounter(5);
            counter.Reset();

            var after = counter.GetValue();

            Assert.Equal(before, after);
        }
    }
}
