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
    public class MetricsManagerUninitializedTests
    {
        [Fact]
        public void FlushAll()
        {
            MetricsManager.Initialize("Test", "Test");
            Assert.Null(Record.Exception(() =>
            {
                CollectTestMetrics("api=Upload");
                CollectTestMetrics("api=Download");
                MetricsManager.FlushAll();
            }));
        }

        [Fact]
        public void FlushIndividualMetricsLogger()
        {
            Assert.Null(Record.Exception(() =>
            {
                CollectTestMetrics("api=Upload").Flush();
                CollectTestMetrics("api=Download").Flush();
                MetricsManager.FlushAll();
            }));
        }

        [Fact]
        public void GetRootMetricsLogger()
        {
            Assert.Null(Record.Exception(() =>
            {
                MetricsManager.GetRootMetricsLogger().IncrementCounter("NumOfExceptions", 1);
                MetricsManager.GetRootMetricsLogger().Flush();
                MetricsManager.FlushAll();
            }));
        }

        [Fact]
        public void HostInfoShouldBeEqualToLocal()
        {
            Assert.Equal(string.Empty, MetricsManager.GetHostInfo());
        }

        [Fact]
        public void ServiceNameShouldBeUninitialized()
        {
            Assert.Equal(string.Empty, MetricsManager.GetServiceName());
        }

        private static MetricsLogger CollectTestMetrics(string dimensions)
        {
            var logger = MetricsManager.GetMetricsLogger(dimensions);

            logger.IncrementCounter("NumOfExceptions", 1);
            logger.IncrementCounter("Invalid@Name@Will@Be@Ignored@Logged@Emit@ErrorMetric", 1);

            return logger;
        }
    }
}