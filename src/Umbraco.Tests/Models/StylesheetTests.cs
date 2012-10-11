using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class StylesheetTests
    {
        [Test]
        public void Can_Create_Stylesheet()
        {
            // Arrange
            var stylesheet = new Stylesheet("/css/styles.css");
            stylesheet.Content = @"body { color:#000; } .bold {font-weight:bold;}";

            Assert.That(stylesheet.Name, Is.EqualTo("styles.css"));
            Assert.That(stylesheet.Alias, Is.EqualTo("styles"));
        }

        [Test]
        public void Can_Get_Properties_From_Css()
        {
            // Arrange
            var stylesheet = new Stylesheet("/css/styles.css");
            stylesheet.Content = @"body { color:#000; } .bold {font-weight:bold;}";

            // Act
            var properties = stylesheet.Properties;

            Assert.That(properties, Is.Not.Null);
            Assert.That(properties.Any(), Is.True);
            Assert.That(properties.Count(), Is.EqualTo(2));
        }
    }
}