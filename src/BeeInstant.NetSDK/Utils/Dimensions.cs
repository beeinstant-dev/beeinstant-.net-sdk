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
using System.Linq;
using System.Text.RegularExpressions;
using static System.String;

namespace BeeInstant.NetSDK.Utils
{
    public static class Dimensions
    {
        private const string Pattern = "^[A-Za-z0-9\\+\\-\\*/:_\\.]+$";
        private static readonly Regex Regex = new Regex(Pattern);

        public static bool IsValidName(string name)
        {
            return Regex.IsMatch(name);
        }

        public static string ExtendAndSerializeDimensions(IDictionary<string, string> rootDimensions,
            string targetDimensions)
        {
            if (IsNullOrEmpty(targetDimensions))
                return Empty;

            var newDimensions = ParseDimensions(targetDimensions);

            if (!newDimensions.Any())
                return Empty;

            foreach (var dim in rootDimensions)
            {
                if (!newDimensions.ContainsKey(dim.Key))
                {
                    newDimensions.Add(dim.Key, dim.Value);
                }
            }

            return SerializeDimensionsToString(newDimensions);
        }

        public static IDictionary<string, string> ParseDimensions(string dimensions)
        {
            var dimensionsMap = new SortedDictionary<string, string>();
            var commaSeparatedDimensions = dimensions.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var commaSeparated in commaSeparatedDimensions)
            {
                var keyValues = commaSeparated.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);

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
            return Join(",", dimensionsToJoin);
        }
    }
}