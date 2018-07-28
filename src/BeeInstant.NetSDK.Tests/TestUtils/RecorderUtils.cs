using System;
using System.Collections.Generic;
using Xunit;

namespace BeeInstant.NetSDK.Tests.TestUtils
{
    public static class RecorderUtils
    {
        public static void AssertRecorderOutput(IList<decimal> expected, Unit unit, string actual, decimal epsilon = 0)
        {
            Assert.True(actual.EndsWith(unit.ToString()));

            var values = actual.Substring(0, actual.Length - unit.ToString().Length)
                                .Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);

            var actualValues = new List<decimal>();

            foreach (var val in values)
            {
                if (Decimal.TryParse(val, out decimal d))
                {
                    actualValues.Add(d);
                }
            }

            Assert.Equal(expected.Count, actualValues.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.True(Math.Abs(expected[i] - actualValues[i]) <= epsilon);
            }
        }
    }
}