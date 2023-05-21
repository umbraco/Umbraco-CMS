// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using Umbraco.Cms.Core.Xml.XPath;
using Umbraco.Cms.Tests.Common.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.CoreXml;

[TestFixture]
public class RenamedRootNavigatorTests
{
    [Test]
    public void StandardNavigator()
    {
        var doc = new XmlDocument();
        doc.LoadXml(@"<root foo=""bar"">
  <a></a>
  <b x=""1""></b>
</root>");
        var nav = doc.CreateNavigator();
        var xml = nav.OuterXml;
        Assert.AreEqual(
            EnsureNativeLineEndings(@"<root foo=""bar"">
  <a></a>
  <b x=""1""></b>
</root>"),
            xml);
    }

    [Test]
    public void StandardNavigatorWithNamespace()
    {
        var doc = new XmlDocument();
        doc.LoadXml(@"<xx:root foo=""bar"" xmlns:xx=""uri"">
  <a></a>
  <b x=""1""></b>
</xx:root>");
        var nav = doc.CreateNavigator();
        var xml = nav.OuterXml;
        Assert.AreEqual(
            EnsureNativeLineEndings(@"<xx:root foo=""bar"" xmlns:xx=""uri"">
  <a></a>
  <b x=""1""></b>
</xx:root>"),
            xml);
    }

    [Test]
    public void RenamedNavigator()
    {
        var doc = new XmlDocument();
        doc.LoadXml(@"<root foo=""bar"">
  <a></a>
  <b x=""1""></b>
</root>");
        var nav = new RenamedRootNavigator(doc.CreateNavigator(), "test");
        var xml = nav.OuterXml;
        Assert.AreEqual(
            EnsureNativeLineEndings(@"<test foo=""bar"">
  <a></a>
  <b x=""1""></b>
</test>"),
            xml);
    }

    private string EnsureNativeLineEndings(string text)
    {
        var useCrLf = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        return useCrLf ? text.CrLf() : text.Lf();
    }

    [Test]
    public void RenamedNavigatorWithNamespace()
    {
        var doc = new XmlDocument();
        doc.LoadXml(@"<xx:root foo=""bar"" xmlns:xx=""uri"">
  <a></a>
  <b x=""1""></b>
</xx:root>");
        var nav = new RenamedRootNavigator(doc.CreateNavigator(), "test");
        var xml = nav.OuterXml;
        Assert.AreEqual(
            EnsureNativeLineEndings(@"<xx:test foo=""bar"" xmlns:xx=""uri"">
  <a></a>
  <b x=""1""></b>
</xx:test>"),
            xml);
    }

    [Test]
    public void RenamedNavigatorProperties()
    {
        var doc = new XmlDocument();
        doc.LoadXml(@"<root foo=""bar"">
  <a></a>
  <b x=""1""></b>
</root>");
        var nav = new RenamedRootNavigator(doc.CreateNavigator(), "test");
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(XPathNodeType.Element, nav.NodeType);
        Assert.AreEqual("test", nav.Name);
        Assert.AreEqual("test", nav.LocalName);
    }

    [Test]
    public void RenamedNavigatorWithNamespaceProperties()
    {
        var doc = new XmlDocument();
        doc.LoadXml(@"<xx:root foo=""bar"" xmlns:xx=""uri"">
  <a></a>
  <b x=""1""></b>
</xx:root>");
        var nav = new RenamedRootNavigator(doc.CreateNavigator(), "test");
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(XPathNodeType.Element, nav.NodeType);
        Assert.AreEqual("xx:test", nav.Name);
        Assert.AreEqual("test", nav.LocalName);
    }
}
