using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;

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

            // Assert
            Assert.That(stylesheet.Name, Is.EqualTo("styles.css"));
            Assert.That(stylesheet.Alias, Is.EqualTo("styles"));
        }

        [Test]
        public void Can_Validate_Stylesheet()
        {
            // Arrange
            var stylesheet = new Stylesheet("/css/styles.css");
            stylesheet.Content = @"body { color:#000; } .bold {font-weight:bold;}";

            // Assert
            Assert.That(stylesheet.IsFileValidCss(), Is.True);
            Assert.That(stylesheet.IsValid(), Is.True);
        }

        [Test]
        public void Can_InValidate_Stylesheet()
        {
            // Arrange
            var stylesheet = new Stylesheet("/css/styles.css");
            stylesheet.Content = @"body { color:#000; } .bold font-weight:bold;}";

            // Assert
            Assert.That(stylesheet.IsFileValidCss(), Is.False);
            Assert.That(stylesheet.IsValid(), Is.True);
        }

        [Test]
        public void Can_Validate_Css3_Stylesheet()
        {
            // Arrange
            var stylesheet = new Stylesheet("/css/styles.css");
            stylesheet.Content = "@media screen and (min-width: 768px) { body {background: red}}";

            // Assert
            Assert.That(stylesheet.IsFileValidCss(), Is.True);
            Assert.That(stylesheet.IsValid(), Is.True);
        }

        [Test]
        public void Can_Get_Properties_From_Css()
        {
            // Arrange
            var stylesheet = new Stylesheet("/css/styles.css");
            stylesheet.Content = @"body { color:#000; } .bold {font-weight:bold;}";

            // Act
            var properties = stylesheet.Properties;

            // Assert
            Assert.That(properties, Is.Not.Null);
            Assert.That(properties.Any(), Is.True);
            Assert.That(properties.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Can_Verify_Property_From_Css()
        {
            // Arrange
            var stylesheet = new Stylesheet("/css/styles.css");
            stylesheet.Content = @"body { color:#000;font-weight:normal; } .bold {font-weight:bold;}";

            // Act
            var properties = stylesheet.Properties;
            var property = properties.FirstOrDefault();

            // Assert
            Assert.That(property, Is.Not.Null);
            Assert.That(property.Alias, Is.EqualTo("body"));
            Assert.That(property.Value, Is.EqualTo("color:#000;\r\nfont-weight:normal;\r\n"));
        }

        [Test]
        public void Can_Verify_Multiple_Properties_From_Css_Selectors()
        {
            // Arrange
            var stylesheet = new Stylesheet("/css/styles.css");
            stylesheet.Content = @".bold, .my-font {font-weight:bold; color:#000; align:left;}
                                    #column-sidebar {
                                        width: auto;
                                        float: none;
                                      }";

            // Act
            var properties = stylesheet.Properties;
            var firstProperty = properties.Any(x => x.Alias == "bold");
            var secondProperty = properties.Any(x => x.Alias == "my-font");

            // Assert
            Assert.That(firstProperty, Is.True);
            Assert.That(secondProperty, Is.True);
        }

        [Test]
        public void Can_Verify_Mixed_Css_Css3_Property_From_Css()
        {
            // Arrange
            var stylesheet = new Stylesheet("/css/styles.css");
            stylesheet.Content = @"@media screen and (min-width: 600px) and (min-width: 900px) {
                                      .class {
                                        background: #666;
                                      }
                                    }";

            // Act
            var properties = stylesheet.Properties;

            // Assert
            Assert.That(stylesheet.IsFileValidCss(), Is.True);
            Assert.That(properties, Is.Not.Null);
            Assert.That(properties.Any(), Is.True);
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            var stylesheet = new Stylesheet("/css/styles.css");
            stylesheet.Content = @"@media screen and (min-width: 600px) and (min-width: 900px) {
                                      .class {
                                        background: #666;
                                      }
                                    }";

            var result = ss.ToStream(stylesheet);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }
    }
}