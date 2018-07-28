using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeeInstant.NetSDK.Tests.TestUtils;
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
            RecorderUtils.AssertRecorderOutput(new[] { 300.0M }, Unit.MilliSecond, first.FlushToString(), 30.0M);
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

    }
}