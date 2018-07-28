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
            Assert.Equal("d.api=Upload,d.location=Hanoi,d.service=ImageSharing", Dimensions.ExtendAndSerializeDimensions(rootDimensions, "location=Hanoi"));
            Assert.Equal("d.api=Download,d.service=ImageSharing", Dimensions.ExtendAndSerializeDimensions(rootDimensions, "api=Download"));
            Assert.Equal("d.api=Download,d.location=Hanoi,d.service=ImageSharing", Dimensions.ExtendAndSerializeDimensions(rootDimensions, "location=Hanoi,api=Download"));
            Assert.Equal("d.api=Upload,d.service=ImageSharing", Dimensions.ExtendAndSerializeDimensions(new Dictionary<string, string>(), "service=ImageSharing, api=Upload"));
        }

        [Fact]
        public void DimensionsHaveValidNames()
        {
            Assert.True(Dimensions.IsValidName("HelloWorld+-*/:_1.2.3"));
            Assert.False(Dimensions.IsValidName("HelloWorld@-*/:_1.2.3"));
        }
    }
}