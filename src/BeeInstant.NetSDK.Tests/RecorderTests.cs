using System.Collections.Generic;
using Xunit;

namespace BeeInstant.NetSDK.Tests
{
    public class RecorderTests
    {
        private readonly Recorder _recorder;

        public RecorderTests() => _recorder = new Recorder(Unit.MilliSecond);

        [Fact]
        public void FlushedStringShouldBeEmptyOnEmptyRecorder()
        {
            Assert.Equal(string.Empty, _recorder.FlushToString());
        }

        public void TestRecordersWithDifferentUnits()
        {

        }

        [Fact]
        public void MergeEmptyRecorderNothingHappens()
        {
            _recorder.Record(1, Unit.MilliSecond);
            var tmpRecorder = new Recorder(Unit.MilliSecond);
            _recorder.Merge(tmpRecorder);

            Assert.Equal("1.0ms", _recorder.FlushToString());
        }

        [Fact]
        public void FlushedRecorderHasNoDataInQueue()
        {
            var recorder = new Recorder(Unit.MilliSecond);
            recorder.Record(1, Unit.MilliSecond);
            recorder.FlushToString();

            Assert.True(string.IsNullOrEmpty(recorder.FlushToString()), "Some data are still left after being flushed");
        }

        private void AssertValuesAndUnit(Recorder recorder, string expected, Unit unit, params double[] values)
        {
            var rec = new Recorder(unit);
            foreach(var val in values)
            {
                rec.Record(val, unit);
            }

            Assert.Equal(expected, rec.FlushToString());
        }
    }
}