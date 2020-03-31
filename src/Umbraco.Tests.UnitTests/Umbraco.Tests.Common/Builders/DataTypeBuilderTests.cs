using NUnit.Framework;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class DataTypeBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            const int testId = 3123;

            var builder = new DataTypeBuilder();

            // Act
            var dtd = builder
                .WithId(testId)
                .Build();

            // Assert
            Assert.AreEqual(testId, dtd.Id);
        }
    }
}
