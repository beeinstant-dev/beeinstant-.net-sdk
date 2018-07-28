using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BeeInstant.NetSDK.Tests.Utils;
using Xunit;

namespace BeeInstant.NetSDK.Tests
{
    public class MetricsCollectorTests
    {
        private readonly string _expectedFlushedCollector = "m.MyTimer=\\d.\\dms,m.MyCounter=99,m.Recorder=100.0b";

        [Fact]
        public void TestCounters()
        {
            var collector = new MetricsCollector();
            var numOfExceptionsMetricName = "NumOfExceptions";
            var succeededMetricName = "Succeeded";

            collector.IncrementCounter(numOfExceptionsMetricName, 0);
            collector.IncrementCounter(numOfExceptionsMetricName, 1);
            collector.IncrementCounter(succeededMetricName, 1);
            collector.IncrementCounter(succeededMetricName, 2);

            var metrics = collector.GetMetrics();

            Assert.Equal(2, metrics.Count);
            Assert.Equal("1", metrics[numOfExceptionsMetricName].FlushToString());
            Assert.Equal("3", metrics[succeededMetricName].FlushToString());
        }

        [Fact]
        public async Task TestStartStopTimers()
        {
            var collector = new MetricsCollector();
            using (var clock = collector.StartTimer("Clock1"))
            {
                await Task.Delay(100);
            }

            using (var timer2 = collector.StartTimer("Clock2"))
            {
                await Task.Delay(200);
            }

            var metrics = collector.GetMetrics();

            Assert.Equal(2, metrics.Count);
            RecorderUtils.AssertRecorderOutput(new[] { 100.0M }, Unit.MilliSecond, metrics["Clock1"].FlushToString(), 10.0M);
            RecorderUtils.AssertRecorderOutput(new[] { 200.0M }, Unit.MilliSecond, metrics["Clock2"].FlushToString(), 10.0M);
        }

        [Fact]
        public void TestRecorders()
        {
            var collector = new MetricsCollector();

            collector.Record("Recorder1", 1.0M, Unit.MilliSecond);
            collector.Record("Recorder1", 11.0M, Unit.MilliSecond);
            collector.Record("Recorder2", 2.0M, Unit.Second);
            collector.Record("Recorder2", 22.0M, Unit.Second);

            var metrics = collector.GetMetrics();

            Assert.Equal(2, metrics.Count);
            RecorderUtils.AssertRecorderOutput(new [] { 1.0M, 11.0M}, Unit.MilliSecond, metrics["Recorder1"].FlushToString());
            RecorderUtils.AssertRecorderOutput(new [] { 2.0M, 22.0M}, Unit.Second, metrics["Recorder2"].FlushToString());
        }

        [Fact]
        public void CollectorShouldIgnoreInvalidMetricNames()
        {
            var collector = new MetricsCollector();
            var invalidName = "Invalid@Name";
            collector.IncrementCounter(invalidName, 1);
            using(collector.StartTimer(invalidName))
            {
                collector.Record(invalidName, 0, Unit.None);
            }

            Assert.Equal(string.Empty, collector.FlushToString());
        }

        [Fact]
        public void FlushedMetricDoesNotHaveData()
        {
            var collector = new MetricsCollector();
            collector.Record("counter", 10M, Unit.Minute);

            collector.FlushToString();

            Assert.Equal(string.Empty, collector.GetMetrics()["counter"].FlushToString());
        }

        [Fact]
        public void CrossMetricMergesAreNotAllowed()
        {
            var collector = new MetricsCollector();
            collector.IncrementCounter("Counter", 1);
            var collector2 = new MetricsCollector();
            collector2.Record("Counter", 10M, Unit.Second);

            collector.Merge(collector2);
        }

        [Fact]
        public void MergeOfTwoCollectorsShouldWork()
        {
            var collector1 = new MetricsCollector();
            var collector2 = new MetricsCollector();

            collector1.IncrementCounter("Counter", 1);
            collector2.IncrementCounter("Counter", 10);

            collector1.Merge(collector2);
            var flushed = collector1.FlushToString();

            Assert.Equal("m.Counter=11", flushed);
        } 
    }
}