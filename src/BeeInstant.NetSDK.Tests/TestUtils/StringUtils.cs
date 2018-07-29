using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace BeeInstant.NetSDK.Tests.Utils
{
    public static class StringUtils
    {
        public static void EnsureMatchesAllMetrics(string target, params string[] regex)
        {
            var targetMetrics = target.Replace("\n", "")
                                        .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var patterns = regex.Select(x => 
                                        x.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));

            foreach(var list in patterns)
            {
                foreach(var expr in list)
                {
                    Assert.True(targetMetrics.Any(x => Regex.IsMatch(x, expr, RegexOptions.IgnoreCase)));
                }
            }
        }
    }
}