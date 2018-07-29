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

using Xunit;

namespace BeeInstant.NetSDK.Tests
{
    public class RecorderTests
    {

        [Fact]
        public void FlushedStringShouldBeEmptyOnEmptyRecorder()
        {
            var recorder = new Recorder(Unit.MilliSecond);
            Assert.Equal(string.Empty, recorder.FlushToString());
        }

        [Fact]
        public void TestRecordersWithDifferentUnits()
        {
            AssertValuesAndUnit("1.0ns", Unit.NanoSecond, 1.0M);
            AssertValuesAndUnit("1.0+2.0us", Unit.MicroSecond, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0ms", Unit.MilliSecond, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0s", Unit.Second, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0m", Unit.Minute, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0h", Unit.Hour, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0b", Unit.Byte, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0kb", Unit.KiloByte, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0mb", Unit.MegaByte, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0gb", Unit.GigaByte, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0tb", Unit.TeraByte, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0bps", Unit.BitPerSecond, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0kbps", Unit.KiloBitPerSecond, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0mbps", Unit.MegaBitPerSecond, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0gbps", Unit.GigaBitPerSecond, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0tbps", Unit.TeraBitPerSecond, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0p", Unit.Percent, 1.0M, 2.0M);
            AssertValuesAndUnit("1.0+2.0", Unit.None, 1.0M, 2.0M);
        }

        [Fact]
        public void MergeEmptyRecorderNothingHappens()
        {
            var recorder = new Recorder(Unit.MilliSecond);
            recorder.Record(1.0M, Unit.MilliSecond);
            var tmpRecorder = new Recorder(Unit.MilliSecond);
            recorder.Merge(tmpRecorder);

            Assert.Equal("1.0ms", recorder.FlushToString());
        }

        [Fact]
        public void FlushedRecorderHasNoDataInQueue()
        {
            var recorder = new Recorder(Unit.MilliSecond);
            recorder.Record(1, Unit.MilliSecond);
            recorder.FlushToString();

            Assert.True(string.IsNullOrEmpty(recorder.FlushToString()), "Some data are still left after being flushed");
        }

        private static void AssertValuesAndUnit(string expected, Unit unit, params decimal[] values)
        {
            var rec = new Recorder(unit);
            foreach (var val in values)
            {
                rec.Record(val, unit);
            }

            Assert.Equal(expected, rec.FlushToString());
        }
    }
}