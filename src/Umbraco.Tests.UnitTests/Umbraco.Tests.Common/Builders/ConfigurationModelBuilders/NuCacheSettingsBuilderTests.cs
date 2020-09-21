using NUnit.Framework;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class NuCacheSettingsBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            const int bTreeBlockSize = 2048;

            var builder = new NuCacheSettingsBuilder();

            // Act
            var nuCacheSettings = builder
                .WithBTreeBlockSize(bTreeBlockSize)
                .Build();

            // Assert
            Assert.AreEqual(bTreeBlockSize, nuCacheSettings.BTreeBlockSize);
        }
    }
}
