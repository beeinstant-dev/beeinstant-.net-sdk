using System.Text;
using Xunit;

namespace BeeInstant.NetSDK.Tests
{
    public class SignatureTests
    {
        [Fact]
        public void ShouldReturnProperHash()
        {
            var data = Encoding.UTF8.GetBytes("Hello");

            var actual = new Signature().Sign(data, "World");

            Assert.Equal("RiiEN2EwRBFNIef615g3wSM2IC9MhQCFSPsiZpNCb1Y=", actual);
        }
    }
}