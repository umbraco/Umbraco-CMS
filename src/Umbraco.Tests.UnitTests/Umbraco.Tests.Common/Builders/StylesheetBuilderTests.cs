using System.IO;
using NUnit.Framework;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class StylesheetBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            var testPath = Path.Combine("css", "styles.css");
            const string testContent = @"body { color:#000; } .bold {font-weight:bold;}";

            var builder = new StylesheetBuilder();

            // Act
            var stylesheet = builder
                .WithPath(testPath)
                .WithContent(testContent)
                .Build();

            // Assert
            Assert.AreEqual(Path.Combine("css", "styles.css"), stylesheet.Path);
            Assert.AreEqual(testContent, stylesheet.Content);
        }
    }
}
