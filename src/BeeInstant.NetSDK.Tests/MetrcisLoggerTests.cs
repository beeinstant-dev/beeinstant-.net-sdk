using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BeeInstant.NetSDK.Abstractions;
using BeeInstant.NetSDK.Tests.Utils;
using Xunit;

namespace BeeInstant.NetSDK.Tests
{
    public class MetricsLoggerTests
    {

        private static object locker = new object();

        [Fact]
        public void EmptyLoggerReturnsEmptyString()
        {
            Assert.Equal(string.Empty, new MetricsLogger().FlushToString());
        }

        [Fact]
        public void TestLogToRootDimensions()
        {
            var logger = CreateTestLogger();
            CollectTestMetrics(logger);
            StringUtils.EnsureMatchesAllMetrics(logger.FlushToString(),
                                                 "d.service=ImageSharing,m.NumOfUploadedImages=1000,m.ImageSize=100.0\\+200.0kb,m.Latency=\\d+.\\dms");

            Assert.Equal(string.Empty, logger.FlushToString());
        }

        [Fact]
        public void ExtendEmptyDimensions()
        {
            var metrics = CreateTestLogger();

            CollectTestMetrics(metrics.ExtendDimensions(""));

            Assert.Equal(string.Empty, metrics.FlushToString());
        }

        [Fact]
        public void TestExtendDimensions()
        {
            var logger = CreateTestLogger();
            var metrics = logger.ExtendDimensions("api=Upload, location=Hanoi");
            CollectTestMetrics(metrics);
            StringUtils.EnsureMatchesAllMetrics(logger.FlushToString(),
                                                "d.api=Upload,d.location=Hanoi,d.service=ImageSharing,m.NumOfUploadedImages=1000,m.ImageSize=100.0\\+200.0kb,m.Latency=\\d+.\\dms");

            CollectTestMetrics(metrics);
            StringUtils.EnsureMatchesAllMetrics(logger.FlushToString(),
                                                "d.api=Upload,d.location=Hanoi,d.service=ImageSharing,m.NumOfUploadedImages=1000,m.ImageSize=100.0\\+200.0kb,m.Latency=\\d+.\\dms");
        }

        [Fact]
        public void TestExtendMultipleDimensions()
        {
            var logger = CreateTestLogger();
            var metric = logger.ExtendMultipleDimensions("api=Upload, location=Hanoi", "api=Download", "api=Download", "");
            CollectTestMetrics(metric);
            StringUtils.EnsureMatchesAllMetrics(logger.FlushToString(),
                                                "d.api=Download,d.service=ImageSharing,m.NumOfUploadedImages=1000,m.ImageSize=100.0\\+200.0kb,m.Latency=\\d+.\\dms",
                                                "d.api=Upload,d.location=Hanoi,d.service=ImageSharing,m.NumOfUploadedImages=1000,m.ImageSize=100.0\\+200.0kb,m.Latency=\\d+.\\dms");
        }

        [Fact]
        public void TestExtendMultipleDimensionsIncludeRoot()
        {
            var logger = CreateTestLogger();
            var metrics = logger.ExtendMultipleDimensionsIncludeRoot("api=Upload, location=Hanoi", "api=Download");
            CollectTestMetrics(metrics);

            StringUtils.EnsureMatchesAllMetrics(logger.FlushToString(),
                                                "d.api=Upload,d.location=Hanoi,d.service=ImageSharing,m.NumOfUploadedImages=1000,m.ImageSize=100.0\\+200.0kb,m.Latency=\\d+.\\dms",
                                                "d.api=Download,d.service=ImageSharing,m.NumOfUploadedImages=1000,m.ImageSize=100.0\\+200.0kb,m.Latency=\\d+.\\dms",
                                                "d.service=ImageSharing,m.NumOfUploadedImages=1000,m.ImageSize=100.0\\+200.0kb,m.Latency=\\d+.\\dms");

            CollectTestMetrics(metrics);

            StringUtils.EnsureMatchesAllMetrics(logger.FlushToString(),
                                                "d.api=Upload,d.location=Hanoi,d.service=ImageSharing,m.NumOfUploadedImages=1000,m.ImageSize=100.0\\+200.0kb,m.Latency=\\d+.\\dms",
                                                "d.api=Download,d.service=ImageSharing,m.NumOfUploadedImages=1000,m.ImageSize=100.0\\+200.0kb,m.Latency=\\d+.\\dms",
                                                "d.service=ImageSharing,m.NumOfUploadedImages=1000,m.ImageSize=100.0\\+200.0kb,m.Latency=\\d+.\\dms");
        }

        [Fact]
        public void LoggingAndFlushingMetricsInMultipleThreads()
        {
            var logger = CreateTestLogger();
            var tasks = new List<Task>();
            var output = new ConcurrentQueue<string>();
            var rnd = new Random();

            tasks.AddRange(PopulateTasks(50, 10, () =>
            {
                logger.IncrementCounter("Counter", 1);
            }));

            tasks.AddRange(PopulateTasks(50, 10, () =>
            {
                logger.Record("Recorder", 1.0M, Unit.Second);
            }));

            tasks.AddRange(PopulateTasks(50, 10, () =>
            {
                var timer = logger.StartTimer("Timer");
                timer.Dispose();
            }));

            tasks.AddRange(PopulateTasks(1, 10, () =>
            {
                output.Enqueue(logger.FlushToString());
            }));

            tasks.Shuffle();

            Parallel.ForEach(tasks, (t) => t.Start());

            Task.WaitAll(tasks.ToArray());

            output.Enqueue(logger.FlushToString());

            var recorderValues = new List<decimal>();
            var timerValues = new List<decimal>();
            var counterValues = new List<decimal>();

            var expectedDimensions = "d.service=ImageSharing,";

            foreach (var logEntry in output.Where(x => !string.IsNullOrEmpty(x))
                                            .Select(x => x.Replace("\n", "")))
            {
                Assert.True(logEntry.StartsWith(expectedDimensions));
                Assert.True(logEntry.Length > expectedDimensions.Length);

                recorderValues.AddRange(AssertAndExtractValues(logEntry, "Recorder", Unit.Second.ToString()).ToList());
                timerValues.AddRange(AssertAndExtractValues(logEntry, "Timer", Unit.MilliSecond.ToString()).ToList());
                counterValues.AddRange(AssertAndExtractValues(logEntry, "Counter").ToList());
            }

            Assert.Equal(500, recorderValues.Count);
            Assert.Equal(500.0M, recorderValues.Sum());
            Assert.Equal(500, timerValues.Count);
            Assert.Equal(500.0M, counterValues.Sum());
        }

        private ICollection<decimal> AssertAndExtractValues(string input, string metricName, string unit = "")
        {
            var recorderValuesString = ExtractMetricValues(metricName, input);

            if (!string.IsNullOrEmpty(recorderValuesString))
            {
                Assert.True(recorderValuesString.EndsWith(unit));
                var decimalString = recorderValuesString.Substring(0, recorderValuesString.Length - unit.Length);
                return ConvertValuesStringToListOfDecimals(decimalString).ToList();
            }

            return new List<decimal>();
        }

        private ICollection<decimal> ConvertValuesStringToListOfDecimals(string input)
        {
            var result = new List<decimal>();

            if (!string.IsNullOrEmpty(input))
            {
                var values = input.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(x => Decimal.Parse(x))
                                  .ToList();

                result.AddRange(values);
            }

            return result;
        }

        private string ExtractMetricValues(string metricName, string input)
        {
            var regex = new Regex($".+m.{metricName}=([^\\n,]+).*");
            var matches = regex.Matches(input);

            if (matches.Any())
            {
                var firstMatch = matches.First();
                if (firstMatch.Groups.Count >= 2)
                {
                    return firstMatch.Groups.ElementAt(1).Value;
                }
            }

            return string.Empty;
        }

        private IList<Task> PopulateTasks(int samples, int totalTasks, Action action)
        {
            var tasks = new List<Task>();

            for (int i = 0; i < totalTasks; i++)
            {
                tasks.Add(new Task(() =>
                {
                    lock (locker)
                    {
                        for (int j = 0; j < samples; j++)
                        {
                            action.Invoke();
                        }
                    }
                }));
            }

            return tasks;
        }

        private MetricsLogger CreateTestLogger() => new MetricsLogger("service=ImageSharing");

        private void CollectTestMetrics(IMetricsComposer metrics)
        {
            metrics.IncrementCounter("NumOfUploadedImages", 1000);
            var timer = metrics.StartTimer("Latency");
            timer.Dispose();

            metrics.Record("ImageSize", 100.0M, Unit.KiloByte);
            metrics.Record("ImageSize", 200.0M, Unit.KiloByte);
        }
    }
}