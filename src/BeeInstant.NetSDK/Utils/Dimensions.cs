using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BeeInstant.NetSDK.Utils
{
    internal static class Dimensions
    {
        private static readonly string _pattern = "^[A-Za-z0-9\\+\\-\\*/:_\\.]+$";
        private static readonly Regex _regex = new Regex(_pattern);

        internal static string ExtendAndSerializeDimensions(Dictionary<string, string> rootDimesions, string targetDimensions)
        {
            if (string.IsNullOrEmpty(targetDimensions))
                return SerializeDimensionsToString(rootDimesions);

            var newDimensions = ParseDimensions(targetDimensions);

            if (newDimensions.Any())
                return string.Empty;

            foreach (var dim in rootDimesions)
            {
                if (!newDimensions.ContainsKey(dim.Key))
                {
                    newDimensions.Add(dim.Key, dim.Value);
                }
            }

            return SerializeDimensionsToString(newDimensions);
        }

        private static Dictionary<String, String> ParseDimensions(string dimensions)
        {
            var dimensionsMap = new Dictionary<String, String>();
            var commaSeparatedDimensions = dimensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var commaSeparated in commaSeparatedDimensions)
            {
                var keyValues = commaSeparated.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (keyValues.Length != 2)
                {
                    //TODO: log error
                    //MetricsManager.reportError("Invalid dimension key=value pair format " + keyValuePair);
                    return new Dictionary<string, string>();
                }

                var key = keyValues[0].Trim().ToLower();
                var val = keyValues[1].Trim();

                if (IsValidName(key) && IsValidName(val))
                {
                    dimensionsMap.Add(key, val);
                }
                else
                {
                    //TODO: log error
                    //MetricsManager.reportError("Invalid dimension key or value pair " + key + "=" + value);
                    return new Dictionary<string, string>();
                }
            }

            return dimensionsMap;
        }

        private static string SerializeDimensionsToString(Dictionary<string, string> dimensions)
        {
            var dimensionsToJoin = dimensions.Select(x => $"d.{x.Key}={x.Value}").ToArray();
            return String.Join(",", dimensionsToJoin);
        }

        private static bool IsValidName(string name)
        {
            return _regex.IsMatch(name);
        }
    }
}