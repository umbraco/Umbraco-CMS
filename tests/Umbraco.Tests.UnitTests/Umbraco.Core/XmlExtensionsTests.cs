// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

/// <summary>
/// Contains unit tests for the methods in the <see cref="XmlExtensions"/> class.
/// </summary>
[TestFixture]
public class XmlExtensionsTests
{
    /// <summary>
    /// Tests the conversion of an XCData element to an XmlNode.
    /// </summary>
    [Test]
    public void XCDataToXmlNode()
    {
        var cdata = new XElement("test", new XCData("hello world"));
        var xdoc = new XmlDocument();

        var xmlNode = cdata.GetXmlNode(xdoc);

        Assert.AreEqual("hello world", xmlNode.InnerText);
    }

    /// <summary>
    /// Tests that an XText element is correctly converted to an XmlNode with the expected inner text.
    /// </summary>
    [Test]
    public void XTextToXmlNode()
    {
        var cdata = new XElement("test", new XText("hello world"));
        var xdoc = new XmlDocument();

        var xmlNode = cdata.GetXmlNode(xdoc);

        Assert.AreEqual("hello world", xmlNode.InnerText);
    }

    /// <summary>
    /// Tests that the ToXmlNode extension method does not modify the original XmlDocument.
    /// </summary>
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
