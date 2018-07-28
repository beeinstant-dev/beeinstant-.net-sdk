using Xunit;

namespace BeeInstant.NetSDK.Tests
{
    public class MetricsCollectorTests
    {

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
        public void FlushedMetricDoesNotHaveData()
        {
            var collector = new MetricsCollector();
            collector.Record("counter", 10M, Unit.Minute);

            collector.FlushToString();
            Assert.Equal(string.Empty, collector.GetMetrics()["counter"].FlushToString());
        }
    }
}