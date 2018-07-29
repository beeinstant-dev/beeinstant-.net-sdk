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
            RecorderUtils.AssertRecorderOutput(new[] {300.0M}, Unit.MilliSecond, first.FlushToString(), 30.0M);
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