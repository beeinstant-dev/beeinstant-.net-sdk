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

using System.Collections.Generic;
using BeeInstant.NetSDK.Utils;
using Xunit;

namespace BeeInstant.NetSDK.Tests.Utils
{
    public class DimensionUtilsTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" somekey ")]
        [InlineData(" somekey = , somekey2 = ")]
        [InlineData(" somekey, somekey2, somekey3 ")]
        [InlineData(" kEY1 = , KEY1   = Tada, HelloWorld   ")]
        [InlineData(" kEY2 = #@, KEY1   = Tada   ")]
        public void ParseEmptyDimensions(string dimensions)
        {
            Assert.Empty(Dimensions.ParseDimensions(dimensions));
        }

        [Fact]
        public void ParseDimensions()
        {
            var dimensions = Dimensions.ParseDimensions(" kEY1 = Coool, KEY1   = Tada   ");
            Assert.True(dimensions.ContainsKey("key1") && dimensions.Values.Contains("Tada"));
        }

        [Fact]
        public void ExtendDimensions()
        {
            var rootDimensions = Dimensions.ParseDimensions("Service=ImageSharing, Api=Upload");
            Assert.Equal(string.Empty, Dimensions.ExtendAndSerializeDimensions(rootDimensions, ""));
            Assert.Equal(string.Empty, Dimensions.ExtendAndSerializeDimensions(rootDimensions, "Nothing"));
            Assert.Equal("d.api=Upload,d.location=Hanoi,d.service=ImageSharing",
                Dimensions.ExtendAndSerializeDimensions(rootDimensions, "location=Hanoi"));
            Assert.Equal("d.api=Download,d.service=ImageSharing",
                Dimensions.ExtendAndSerializeDimensions(rootDimensions, "api=Download"));
            Assert.Equal("d.api=Download,d.location=Hanoi,d.service=ImageSharing",
                Dimensions.ExtendAndSerializeDimensions(rootDimensions, "location=Hanoi,api=Download"));
            Assert.Equal("d.api=Upload,d.service=ImageSharing",
                Dimensions.ExtendAndSerializeDimensions(new Dictionary<string, string>(),
                    "service=ImageSharing, api=Upload"));
        }

        [Fact]
        public void DimensionsHaveValidNames()
        {
            Assert.True(Dimensions.IsValidName("HelloWorld+-*/:_1.2.3"));
            Assert.False(Dimensions.IsValidName("HelloWorld@-*/:_1.2.3"));
        }
    }
}