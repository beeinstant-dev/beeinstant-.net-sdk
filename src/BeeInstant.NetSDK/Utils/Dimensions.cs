using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BeeInstant.NetSDK.Utils
{
    public static class Dimensions
    {
        private static readonly string _pattern = "^[A-Za-z0-9\\+\\-\\*/:_\\.]+$";
        private static readonly Regex _regex = new Regex(_pattern);

        public static bool IsValidName(string name)
        {
            return _regex.IsMatch(name);
        }

        public static string ExtendAndSerializeDimensions(IDictionary<string, string> rootDimensions, string targetDimensions)
        {
            if (string.IsNullOrEmpty(targetDimensions))
                return string.Empty;

            var newDimensions = ParseDimensions(targetDimensions);

            if (!newDimensions.Any())
                return string.Empty;

            foreach (var dim in rootDimensions)
            {
                if (!newDimensions.ContainsKey(dim.Key))
                {
                    newDimensions.Add(dim.Key, dim.Value);
                }
            }

            return SerializeDimensionsToString(newDimensions);
        }

        public static IDictionary<String, String> ParseDimensions(string dimensions)
        {
            var dimensionsMap = new SortedDictionary<String, String>();
            var commaSeparatedDimensions = dimensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var commaSeparated in commaSeparatedDimensions)
            {
                var keyValues = commaSeparated.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                if (keyValues.Length != 2)
                {
                    MetricsManager.ReportError("Invalid dimension key=value pair format");
                    return new Dictionary<string, string>();
                }

                var key = keyValues[0].Trim().ToLower();
                var val = keyValues[1].Trim();

                if (IsValidName(key) && IsValidName(val))
                {
                    dimensionsMap.AddOrUpdate(key, val);
                }
                else
                {
                    MetricsManager.ReportError("Invalid dimension key or value pair " + key + "=" + val);
                    return new Dictionary<string, string>();
                }
            }

            return dimensionsMap;
        }

        public static string SerializeDimensionsToString(IDictionary<string, string> dimensions)
        {
            var dimensionsToJoin = dimensions.Select(x => $"d.{x.Key}={x.Value}").ToArray();
            return String.Join(",", dimensionsToJoin);
        }

    }
}