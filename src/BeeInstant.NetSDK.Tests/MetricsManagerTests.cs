using System;
using System.Linq;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace BeeInstant.NetSDK.Tests
{
    public class MetricsManagerTests : IDisposable
    {
        private readonly string TestServiceName = "ImageSharing";
        private readonly string TestHostName = "test.beeinstant.com";
        private readonly string TestEnvironment = "Test";
        private readonly FluentMockServer _server = FluentMockServer.Start("http://localhost:8989");

        public MetricsManagerTests()
        {
            MetricsManager.Initialize(TestServiceName, TestEnvironment, TestHostName);
            AddExpectations("/PutMetric*");
        }

        [Fact]
        public void RootMetricLoggerResposnes()
        {
            var logger = MetricsManager.GetRootMetricsLogger();
            logger.IncrementCounter("NumOfExceptions", 1);
            logger.Flush();

            var responses = _server.FindLogEntries(Request.Create().WithPath("/PutMetric*").UsingPost()).ToList();
            Assert.NotEmpty(responses);
            Assert.Equal("d.env=Test,d.service=ImageSharing,m.NumOfExceptions=1\n", responses.First().RequestMessage.Body);
        }

        [Fact]
        public void ExtendInvalidDimensionsIgnoreAndReportError()
        {
            var logger = MetricsManager.GetMetricsLogger("api=Upload");
            logger.ExtendDimensions("invalid-dimensions").IncrementCounter("NumOfExceptions", 1);
            logger.ExtendDimensions("invalid=@dimensions").IncrementCounter("NumOfExceptions", 1);
            MetricsManager.FlushAll();

            var responses = _server.FindLogEntries(Request.Create().WithPath("/PutMetric*").UsingPost()).ToList();
            Assert.NotEmpty(responses);
            Assert.Equal("d.env=Test,d.service=ImageSharing,m.MetricErrors=2\n", responses.First().RequestMessage.Body);
        }

        [Fact]
        public void ShouldHaveCorrectEnvironment()
        {
            Assert.Equal(TestEnvironment, MetricsManager.GetEnvironment());
        }

        [Fact]
        public void InvalidSerivceNameShouldThrow()
        {
            Assert.Throws(typeof(ArgumentException), () =>
            {
                MetricsManager.Initialize("Invalid@Service@Name");
            });
        }

        private MetricsLogger CollectTestMetrics(string dimensions)
        {
            var logger = MetricsManager.GetMetricsLogger(dimensions);
            logger.IncrementCounter("NumOfExceptions", 1);
            logger.IncrementCounter("Invalid@Name@Will@Be@Ignored@Logged@Emit@ErrorMetric", 1);
            return logger;
        }

        public void Dispose()
        {
            _server.Stop();
            _server.ResetLogEntries();
            MetricsManager.Shutdown();
        }

        private void AddExpectations(string path)
        {
            _server.Given(Request.Create().WithPath(path))
                     .RespondWith(Response.Create().WithStatusCode(200)
                            .WithHeader("content-type", "application/json"));

        }
    }
}