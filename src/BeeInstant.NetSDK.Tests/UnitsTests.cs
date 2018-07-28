using Xunit;

namespace BeeInstant.NetSDK.Tests
{
    public class UnitsTests
    {
        [Fact]
        public void SameUnitsShouldBeEqual()
        {
            var byte1 = Unit.Byte;
            var byte2 = Unit.Byte;
            var gbyte = Unit.GigaByte;

            Assert.True(byte1.Equals(byte2));
            Assert.False(byte2.Equals(gbyte));
        }
    }
}