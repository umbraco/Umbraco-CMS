// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

[TestFixture]
public class XmlExtensionsTests
{
    [Test]
    public void XCDataToXmlNode()
    {
        var cdata = new XElement("test", new XCData("hello world"));
        var xdoc = new XmlDocument();

        var xmlNode = cdata.GetXmlNode(xdoc);

        Assert.AreEqual("hello world", xmlNode.InnerText);
    }

    [Test]
    public void XTextToXmlNode()
    {
        var cdata = new XElement("test", new XText("hello world"));
        var xdoc = new XmlDocument();

        var xmlNode = cdata.GetXmlNode(xdoc);

        Assert.AreEqual("hello world", xmlNode.InnerText);
    }

    [Test]
    public void ToXmlNodeIsNonDestructive()
    {
        const string xml = "<root><foo attr=\"123\">hello</foo><bar>world</bar></root>";

        var cdata = new XElement("test", new XText("hello world"));
        var xdoc = new XmlDocument();
        xdoc.LoadXml(xml);

        var xmlNode = cdata.GetXmlNode(xdoc);

        Assert.AreEqual("hello world", xmlNode.InnerText);
        Assert.AreEqual(xml, xdoc.OuterXml);
    }
}
