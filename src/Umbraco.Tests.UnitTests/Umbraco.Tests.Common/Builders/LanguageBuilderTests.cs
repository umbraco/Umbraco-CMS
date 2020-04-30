using NUnit.Framework;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class LanguageBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            var builder = new LanguageBuilder();

            // Act
            var language = builder
                .WithCultureInfo("en-GB")
                .Build();

            // Assert
            Assert.AreEqual("GB", language.IsoCode);
            Assert.AreEqual("en", language.CultureName);
        }
    }
}
