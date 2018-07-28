using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BeeInstant.NetSDK.Tests
{
    public class TimerTests
    {
        [Fact]
        public void EmptyTimerFlushedEmptyString()
        {
            var actual = new Timer().FlushToString();
            Assert.Equal(string.Empty, actual);
        }

        [Fact]
        public void StopTimerBeforeStartNothingHappens()
        {
            var timer = new Timer();
            timer.StopTimer();

            Assert.True(string.IsNullOrEmpty(timer.FlushToString()), "Timer has unexpected data. It should be empty.");
        }

        [Fact]
        public async Task MergeAfterStartAndStop()
        {
            var first = new Timer();
            var second = new Timer();

            second.StartTimer();
            await Task.Delay(300);
            second.StopTimer();

            first.Merge(second);
            AssertRecorderOutput(new[] { 300.0M }, Unit.MilliSecond, first.FlushToString(), 30.0M);
        }

        [Fact]
        public async Task SecondFlushReturnsEmptyString()
        {
            var timer = new Timer();

            timer.StartTimer();
            await Task.Delay(100);
            timer.StopTimer();

            timer.FlushToString();

            Assert.Equal(string.Empty, timer.FlushToString());
        }

        [Fact]
        public void MergeEmptyTimerNothingHappens()
        {
            var timer = new Timer().Merge(new Timer());

            Assert.Equal(string.Empty, timer.FlushToString());
        }

        private void AssertRecorderOutput(IList<decimal> expected, Unit unit, string actual, decimal epsilon)
        {
            Assert.True(actual.EndsWith(unit.ToString()));

            var values = actual.Substring(0, actual.Length - unit.ToString().Length)
                                .Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);

            var actualValues = new List<decimal>();

            foreach (var val in values)
            {
                if (Decimal.TryParse(val, out decimal d))
                {
                    actualValues.Add(d);
                }
            }

            Assert.Equal(expected.Count, actualValues.Count);
            
            for(int i = 0; i < expected.Count; i++)
            {
                Assert.True(Math.Abs(expected[i] - actualValues[i]) <= epsilon);
            }
        }

    }
}