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
        private const string TestServiceName = "ImageSharing";
        private const string TestHostName = "test.beeinstant.com";
        private const string TestEnvironment = "Test";
        private readonly FluentMockServer _server = FluentMockServer.Start("http://localhost:8989");

        public MetricsManagerTests()
        {
            MetricsManager.Initialize(TestServiceName, TestEnvironment, TestHostName);
            AddExpectations("/PutMetric*");
        }

        //[Fact]
        //public void RootMetricLoggerResponses()
        //{
        //    var logger = MetricsManager.GetRootMetricsLogger();
        //    logger.IncrementCounter("NumOfExceptions", 1);
        //    logger.Flush();

        //    var responses = _server.FindLogEntries(Request.Create().WithPath("/PutMetric*").UsingPost()).ToList();
        //    Assert.NotEmpty(responses);
        //    Assert.Equal("d.env=Test,d.service=ImageSharing,m.NumOfExceptions=1",
        //        responses.First().RequestMessage.Body);
        //}

        //[Fact]
        //public void ExtendInvalidDimensionsIgnoreAndReportError()
        //{
        //    var logger = MetricsManager.GetMetricsLogger("api=Upload");
        //    logger.ExtendDimensions("invalid-dimensions").IncrementCounter("NumOfExceptions", 1);
        //    logger.ExtendDimensions("invalid=@dimensions").IncrementCounter("NumOfExceptions", 1);
        //    MetricsManager.FlushAll();

        //    var responses = _server.FindLogEntries(Request.Create().WithPath("/PutMetric*").UsingPost()).ToList();
        //    Assert.NotEmpty(responses);
        //    Assert.Equal("d.env=Test,d.service=ImageSharing,m.MetricErrors=2", responses.First().RequestMessage.Body);
        //}

        [Fact]
        public void ShouldHaveCorrectEnvironment()
        {
            Assert.Equal(TestEnvironment, MetricsManager.GetEnvironment());
        }

        [Fact]
        public void InvalidSerivceNameShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => { MetricsManager.Initialize("Invalid@Service@Name"); });
        }

        public void Dispose()
        {
            _server.Stop();
            _server.ResetLogEntries();
            //MetricsManager.Shutdown();
        }

        private void AddExpectations(string path)
        {
            _server.Given(Request.Create().WithPath(path))
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("content-type", "application/json"));
        }
    }
}