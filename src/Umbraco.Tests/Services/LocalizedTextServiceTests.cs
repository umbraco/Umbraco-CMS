using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Core.Services;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    public class LocalizedTextServiceTests
    {

        [Test]
        public void Using_Dictionary_Returns_Text_With_Area()
        {
            var culture = CultureInfo.GetCultureInfo("en-US");
            var txtService = new LocalizedTextService(
                new Dictionary<CultureInfo, IDictionary<string, IDictionary<string, string>>>
                {
                    {
                        culture, new Dictionary<string, IDictionary<string, string>>
                        {
                            {
                                "testArea", new Dictionary<string, string>
                                {
                                    {"testKey", "testValue"}
                                }
                            }
                        }
                    }
                });

            var result = txtService.Localize("testArea/testKey", culture);

            Assert.AreEqual("testValue", result);
        }

        [Test]
        public void Using_Dictionary_Returns_Text_Without_Area()
        {
            var culture = CultureInfo.GetCultureInfo("en-US");
            var txtService = new LocalizedTextService(
                new Dictionary<CultureInfo, IDictionary<string, IDictionary<string, string>>>
                {
                    {
                        culture, new Dictionary<string, IDictionary<string, string>>
                        {
                            {
                                "testArea", new Dictionary<string, string>
                                {
                                    {"testKey", "testValue"}
                                }
                            }
                        }
                    }
                });

            var result = txtService.Localize("testKey", culture);

            Assert.AreEqual("testValue", result);
        }

        [Test]
        public void Using_Dictionary_Returns_Default_Text_When_Not_Found_With_Area()
        {
            var culture = CultureInfo.GetCultureInfo("en-US");
            var txtService = new LocalizedTextService(
                new Dictionary<CultureInfo, IDictionary<string, IDictionary<string, string>>>
                {
                    {
                        culture, new Dictionary<string, IDictionary<string, string>>
                        {
                            {
                                "testArea", new Dictionary<string, string>
                                {
                                    {"testKey", "testValue"}
                                }
                            }
                        }
                    }
                });

            var result = txtService.Localize("testArea/doNotFind", culture);

            //NOTE: Based on how legacy works, the default text does not contain the area, just the key
            Assert.AreEqual("[doNotFind]", result);
        }

        [Test]
        public void Using_Dictionary_Returns_Default_Text_When_Not_Found_Without_Area()
        {
            var culture = CultureInfo.GetCultureInfo("en-US");
            var txtService = new LocalizedTextService(
                new Dictionary<CultureInfo, IDictionary<string, IDictionary<string, string>>>
                {
                    {
                        culture, new Dictionary<string, IDictionary<string, string>>
                        {
                            {
                                "testArea", new Dictionary<string, string>
                                {
                                    {"testKey", "testValue"}
                                }
                            }
                        }
                    }
                });

            var result = txtService.Localize("doNotFind", culture);

            Assert.AreEqual("[doNotFind]", result);
        }

        [Test]
        public void Using_XDocument_Returns_Text_With_Area()
        {
            var culture = CultureInfo.GetCultureInfo("en-US");
            var txtService = new LocalizedTextService(
                new Dictionary<CultureInfo, Lazy<XDocument>>
                {
                    {
                        culture, new Lazy<XDocument>(() => new XDocument(
                            new XElement("area", new XAttribute("alias", "testArea"),
                                new XElement("key", new XAttribute("alias", "testKey"), 
                                    "testValue"))))
                    }
                });

            var result = txtService.Localize("testArea/testKey", culture);

            Assert.AreEqual("testValue", result);
        }

        [Test]
        public void Using_XDocument_Returns_Text_Without_Area()
        {
            var culture = CultureInfo.GetCultureInfo("en-US");
            var txtService = new LocalizedTextService(
                new Dictionary<CultureInfo, Lazy<XDocument>>
                {
                    {
                        culture, new Lazy<XDocument>(() => new XDocument(
                            new XElement("area", new XAttribute("alias", "testArea"),
                                new XElement("key", new XAttribute("alias", "testKey"), 
                                    "testValue"))))
                    }
                });

            var result = txtService.Localize("testKey", culture);

            Assert.AreEqual("testValue", result);
        }

        [Test]
        public void Using_XDocument_Returns_Default_Text_When_Not_Found_With_Area()
        {
            var culture = CultureInfo.GetCultureInfo("en-US");
            var txtService = new LocalizedTextService(
                new Dictionary<CultureInfo, Lazy<XDocument>>
                {
                    {
                        culture, new Lazy<XDocument>(() => new XDocument(
                            new XElement("area", new XAttribute("alias", "testArea"),
                                new XElement("key", new XAttribute("alias", "testKey"), 
                                    "testValue"))))
                    }
                });

            var result = txtService.Localize("testArea/doNotFind", culture);

            //NOTE: Based on how legacy works, the default text does not contain the area, just the key
            Assert.AreEqual("[doNotFind]", result);
        }

        [Test]
        public void Using_XDocument_Returns_Default_Text_When_Not_Found_Without_Area()
        {
            var culture = CultureInfo.GetCultureInfo("en-US");
            var txtService = new LocalizedTextService(
                new Dictionary<CultureInfo, Lazy<XDocument>>
                {
                    {
                        culture, new Lazy<XDocument>(() => new XDocument(
                            new XElement("area", new XAttribute("alias", "testArea"),
                                new XElement("key", new XAttribute("alias", "testKey"), 
                                    "testValue"))))
                    }
                });

            var result = txtService.Localize("doNotFind", culture);

            Assert.AreEqual("[doNotFind]", result);
        }

        [Test]
        public void Using_Dictionary_Throws_When_No_Culture_Found()
        {
            var culture = CultureInfo.GetCultureInfo("en-US");
            var txtService = new LocalizedTextService(
                new Dictionary<CultureInfo, IDictionary<string, IDictionary<string, string>>>
                {
                    {
                        culture, new Dictionary<string, IDictionary<string, string>>
                        {
                            {
                                "testArea", new Dictionary<string, string>
                                {
                                    {"testKey", "testValue"}
                                }
                            }
                        }
                    }
                });

            Assert.Throws<NullReferenceException>(() => txtService.Localize("testArea/testKey", CultureInfo.GetCultureInfo("en-AU")));            
        }

        [Test]
        public void Using_XDocument_Throws_When_No_Culture_Found()
        {
            var culture = CultureInfo.GetCultureInfo("en-US");
            var txtService = new LocalizedTextService(
                new Dictionary<CultureInfo, Lazy<XDocument>>
                {
                    {
                        culture, new Lazy<XDocument>(() => new XDocument(
                            new XElement("area", new XAttribute("alias", "testArea"),
                                new XElement("key", new XAttribute("alias", "testKey"), 
                                    "testValue"))))
                    }
                });

            Assert.Throws<NullReferenceException>(() => txtService.Localize("testArea/testKey", CultureInfo.GetCultureInfo("en-AU")));
        }
    }
}
