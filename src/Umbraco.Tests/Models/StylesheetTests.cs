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
        public void Can_Add_Property()
        {
            // Arrange
            var stylesheet = new Stylesheet("/css/styles.css") {Content = @"body { color:#000; } .bold {font-weight:bold;}"};

            stylesheet.AddProperty(new StylesheetProperty("Test", "p", "font-weight:bold; font-family:Arial;"));

            // Assert
            Assert.AreEqual(1, stylesheet.Properties.Count());
            Assert.AreEqual("Test", stylesheet.Properties.Single().Name);
            Assert.AreEqual("p", stylesheet.Properties.Single().Alias);
            Assert.AreEqual("font-weight:bold; font-family:Arial;", stylesheet.Properties.Single().Value);
        }

        [Test]
        public void Can_Remove_Property()
        {
            // Arrange
            var stylesheet = new Stylesheet("/css/styles.css") { Content = @"body { color:#000; } /**umb_name:Hello*/p{font-size:2em;} .bold {font-weight:bold;}" };

            
            Assert.AreEqual(1, stylesheet.Properties.Count());

            stylesheet.RemoveProperty("Hello");

            Assert.AreEqual(0, stylesheet.Properties.Count());
            Assert.AreEqual(@"body { color:#000; }  .bold {font-weight:bold;}", stylesheet.Content);
        }

        [Test]
        public void Can_Update_Property()
        {
            // Arrange
            var stylesheet = new Stylesheet("/css/styles.css") { Content = @"body { color:#000; } /**umb_name:Hello*/p{font-size:2em;} .bold {font-weight:bold;}" };

            var prop = stylesheet.Properties.Single();
            prop.Alias = "li";
            prop.Value = "font-size:5em;";

            //re-get
            prop = stylesheet.Properties.Single();
            Assert.AreEqual("li", prop.Alias);
            Assert.AreEqual("font-size:5em;", prop.Value);
            Assert.AreEqual("body { color:#000; } /**umb_name:Hello*/\r\nli{font-size:5em;} .bold {font-weight:bold;}", stylesheet.Content);
        }

        [Test]
        public void Can_Get_Properties_From_Css()
        {
            // Arrange
            var stylesheet = new Stylesheet("/css/styles.css");
            stylesheet.Content = @"body { color:#000; } .bold {font-weight:bold;} /**umb_name:Hello */ p { font-size: 1em; } /**umb_name:testing123*/ li:first-child {padding:0px;}";

            // Act
            var properties = stylesheet.Properties;

            Assert.AreEqual(2, properties.Count());
            Assert.AreEqual("Hello", properties.First().Name);
            Assert.AreEqual("font-size: 1em;", properties.First().Value);
            Assert.AreEqual("p", properties.First().Alias);
            Assert.AreEqual("testing123", properties.Last().Name);
            Assert.AreEqual("padding:0px;", properties.Last().Value);
            Assert.AreEqual("li:first-child", properties.Last().Alias);
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