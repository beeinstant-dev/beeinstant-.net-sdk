using System.Collections.Generic;
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
            AssertValuesAndUnit("1.0+2.0us", Unit.MicroSecond, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0ms", Unit.MilliSecond, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0s", Unit.Second, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0m", Unit.Minute, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0h", Unit.Hour, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0b", Unit.Byte, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0kb", Unit.KiloByte, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0mb", Unit.MegaByte, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0gb", Unit.GigaByte, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0tb", Unit.TeraByte, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0bps", Unit.BitPerSecond, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0kbps", Unit.KiloBitPerSecond, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0mbps", Unit.MegaBitPerSecond, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0gbps", Unit.GigaBitPerSecond, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0tbps", Unit.TeraBitPerSecond, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0p", Unit.Percent, new[] { 1.0M, 2.0M });
            AssertValuesAndUnit("1.0+2.0", Unit.None, new[] { 1.0M, 2.0M });
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

        private void AssertValuesAndUnit(string expected, Unit unit, params decimal[] values)
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