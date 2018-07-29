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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BeeInstant.NetSDK.Abstractions;
using BeeInstant.NetSDK.Tests.TestUtils;
using Xunit;

namespace BeeInstant.NetSDK.Tests
{
    public class MetricsLoggerTests
    {
        private static readonly object Locker = new object();

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
            var metric =
                logger.ExtendMultipleDimensions("api=Upload, location=Hanoi", "api=Download", "api=Download", "");
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

            tasks.AddRange(PopulateTasks(50, 10, () => { logger.IncrementCounter("Counter", 1); }));

            tasks.AddRange(PopulateTasks(50, 10, () => { logger.Record("Recorder", 1.0M, Unit.Second); }));

            tasks.AddRange(PopulateTasks(50, 10, () =>
            {
                var timer = logger.StartTimer("Timer");
                timer.Dispose();
            }));

            tasks.AddRange(PopulateTasks(1, 10, () => { output.Enqueue(logger.FlushToString()); }));

            tasks.Shuffle();

            Parallel.ForEach(tasks, (t) => t.Start());

            Task.WaitAll(tasks.ToArray());

            output.Enqueue(logger.FlushToString());

            var recorderValues = new List<decimal>();
            var timerValues = new List<decimal>();
            var counterValues = new List<decimal>();

            var expectedDimensions = "d.service=ImageSharing,";

            foreach (var logEntry in output.Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Replace("\n", string.Empty).Replace("\r", string.Empty)))
            {
                Assert.StartsWith(expectedDimensions, logEntry);
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

        private static IEnumerable<decimal> AssertAndExtractValues(string input, string metricName, string unit = "")
        {
            var recorderValuesString = ExtractMetricValues(metricName, input);

            if (!string.IsNullOrEmpty(recorderValuesString))
            {
                Assert.EndsWith(unit, recorderValuesString);
                var decimalString = recorderValuesString.Substring(0, recorderValuesString.Length - unit.Length);
                return ConvertValuesStringToListOfDecimals(decimalString).ToList();
            }

            return new List<decimal>();
        }

        private static IEnumerable<decimal> ConvertValuesStringToListOfDecimals(string input)
        {
            var result = new List<decimal>();

            if (string.IsNullOrEmpty(input))
            {
                return result;
            }

            var values = input.Split(new[] {"+"}, StringSplitOptions.RemoveEmptyEntries)
                .Select(decimal.Parse)
                .ToList();

            result.AddRange(values);

            return result;
        }

        private static string ExtractMetricValues(string metricName, string input)
        {
            var regex = new Regex($".+m.{metricName}=([^\\n,]+).*");
            var matches = regex.Matches(input);

            if (!matches.Any())
            {
                return string.Empty;
            }

            var firstMatch = matches.First();
            return firstMatch.Groups.Count >= 2 ? firstMatch.Groups.ElementAt(1).Value : string.Empty;
        }

        private static IEnumerable<Task> PopulateTasks(int samples, int totalTasks, Action action)
        {
            var tasks = new List<Task>();

            for (var i = 0; i < totalTasks; i++)
            {
                tasks.Add(new Task(() =>
                {
                    lock (Locker)
                    {
                        for (var j = 0; j < samples; j++)
                        {
                            action.Invoke();
                        }
                    }
                }));
            }

            return tasks;
        }

        private static MetricsLogger CreateTestLogger() => new MetricsLogger("service=ImageSharing");

        private static void CollectTestMetrics(IMetricsComposer metrics)
        {
            metrics.IncrementCounter("NumOfUploadedImages", 1000);
            var timer = metrics.StartTimer("Latency");
            timer.Dispose();

            metrics.Record("ImageSize", 100.0M, Unit.KiloByte);
            metrics.Record("ImageSize", 200.0M, Unit.KiloByte);
        }
    }
}