using System;
using System.Net;
using Xunit;

namespace BeeInstant.NetSDK.Tests
{
    public class MetricsManagerUninitializedTests
    {
        public void FlushAll()
        {
            Assert.Null(Record.Exception(() =>
            {
                CollectTestMetrics("api=Upload");
                CollectTestMetrics("api=Download");
                MetricsManager.FlushAll();
            }));
        }

        public void FlushIndividualMetricsLogger()
        {
            Assert.Null(Record.Exception(() =>
            {
                CollectTestMetrics("api=Upload").Flush();
                CollectTestMetrics("api=Download").Flush();
                MetricsManager.FlushAll();
            }));
        }

        public void GetRootMetricsLogger()
        {
            Assert.Null(Record.Exception(() =>
            {
                MetricsManager.GetRootMetricsLogger().IncrementCounter("NumOfExceptions", 1);
                MetricsManager.GetRootMetricsLogger().Flush();
                MetricsManager.FlushAll();
            }));
        }

        public void HostInfoShouldBeEqualToLocal()
        {
            Assert.Equal(string.Empty, MetricsManager.GetHostInfo());
        }

        public void ServiceNameShouldBeUninitialized()
        {
            Assert.Equal(string.Empty, MetricsManager.GetServiceName());
        }

        private MetricsLogger CollectTestMetrics(string dimensions)
        {
            var logger = MetricsManager.GetMetricsLogger(dimensions);

            logger.IncrementCounter("NumOfExceptions", 1);
            logger.IncrementCounter("Invalid@Name@Will@Be@Ignored@Logged@Emit@ErrorMetric", 1);

            return logger;
        }
    }
}