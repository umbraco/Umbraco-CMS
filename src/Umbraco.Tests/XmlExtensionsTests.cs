using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests
{
    [TestFixture]
    public class XmlExtensionsTests
    {
        [Test]
        public void XCDataToXmlNode()
        {
            var cdata = new XElement("test", new XCData("hello world"));
            var xdoc = new XmlDocument();

            var xmlNode = cdata.GetXmlNode(xdoc);

            Assert.AreEqual(xmlNode.InnerText, "hello world");
        }

        [Test]
        public void XTextToXmlNode()
        {
            var cdata = new XElement("test", new XText("hello world"));
            var xdoc = new XmlDocument();

            var xmlNode = cdata.GetXmlNode(xdoc);

            Assert.AreEqual(xmlNode.InnerText, "hello world");
        }

        [Test]
        public void ToXmlNodeIsNonDestructive()
        {
            const string xml = "<root><foo attr=\"123\">hello</foo><bar>world</bar></root>";

            var cdata = new XElement("test", new XText("hello world"));
            var xdoc = new XmlDocument();
            xdoc.LoadXml(xml);

            var xmlNode = cdata.GetXmlNode(xdoc);

            Assert.AreEqual(xmlNode.InnerText, "hello world");
            Assert.AreEqual(xml, xdoc.OuterXml);
        }
    }
}