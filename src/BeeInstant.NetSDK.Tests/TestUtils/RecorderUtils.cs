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
using System.Collections.Generic;
using Xunit;

namespace BeeInstant.NetSDK.Tests.TestUtils
{
    public static class RecorderUtils
    {
        public static void AssertRecorderOutput(IList<decimal> expected, Unit unit, string actual, decimal epsilon = 0)
        {
            Assert.EndsWith(unit.ToString(), actual);

            var values = actual.Substring(0, actual.Length - unit.ToString().Length)
                .Split(new[] {'+'}, StringSplitOptions.RemoveEmptyEntries);

            var actualValues = new List<decimal>();

            foreach (var val in values)
            {
                if (decimal.TryParse(val, out var d))
                {
                    actualValues.Add(d);
                }
            }

            Assert.Equal(expected.Count, actualValues.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Assert.True(Math.Abs(expected[i] - actualValues[i]) <= epsilon);
            }
        }
    }
}