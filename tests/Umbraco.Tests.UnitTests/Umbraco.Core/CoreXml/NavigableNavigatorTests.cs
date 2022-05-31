// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using NUnit.Framework;
using Umbraco.Cms.Core.Xml;
using Umbraco.Cms.Core.Xml.XPath;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.CoreXml;

[TestFixture]
public class NavigableNavigatorTests
{
    [Test]
    public void NewNavigatorIsAtRoot()
    {
        const string xml = @"<root><item1 /><item2 /></root>";
        var doc = XmlHelper.CreateXPathDocument(xml);
        var nav = doc.CreateNavigator();

        Assert.AreEqual(XPathNodeType.Root, nav.NodeType);
        Assert.AreEqual(string.Empty, nav.Name);

        var source = new TestSource5();
        nav = new NavigableNavigator(source);

        Assert.AreEqual(XPathNodeType.Root, nav.NodeType);
        Assert.AreEqual(string.Empty, nav.Name);
    }

    [Test]
    public void NativeXmlValues()
    {
        const string xml = @"<root>
    <wrap>
        <item1 />
        <item2></item2>
        <item2a> </item2a>
        <item2b>
        </item2b>
        <item2c><![CDATA[
        ]]></item2c>
        <item3>blah</item3>
        <item3a>
            blah
        </item3a>
        <item4>
            <subitem x=""1"">bam</subitem>
        </item4>
        <item5><![CDATA[
        ]]></item5>
    </wrap>
</root>";
        var doc = XmlHelper.CreateXPathDocument(xml);
        var nav = doc.CreateNavigator();

        NavigatorValues(nav, true);
    }

    [Test]
    public void NavigableXmlValues()
    {
        var source = new TestSource6();
        var nav = new NavigableNavigator(source);

        NavigatorValues(nav, false);
    }

    private static void NavigatorValues(XPathNavigator nav, bool native)
    {
        // in non-native we can't have Value dump everything, else
        // we'd dump the entire database? Makes not much sense.
        Assert.AreEqual(native ? "\n        blah\n            blah\n        bam\n        " : string.Empty, nav.Value.Lf()); // !!
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual("root", nav.Name);
        Assert.AreEqual(native ? "\n        blah\n            blah\n        bam\n        " : string.Empty, nav.Value.Lf()); // !!
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual("wrap", nav.Name);
        Assert.AreEqual(native ? "\n        blah\n            blah\n        bam\n        " : string.Empty, nav.Value.Lf()); // !!

        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual("item1", nav.Name);
        Assert.AreEqual(string.Empty, nav.Value);
        Assert.IsFalse(nav.MoveToFirstChild());

        Assert.IsTrue(nav.MoveToNext());
        Assert.AreEqual("item2", nav.Name);
        Assert.AreEqual(string.Empty, nav.Value);
        Assert.IsFalse(nav.MoveToFirstChild()); // !!

        Assert.IsTrue(nav.MoveToNext());
        Assert.AreEqual("item2a", nav.Name);
        Assert.AreEqual(string.Empty, nav.Value);
        Assert.IsFalse(nav.MoveToFirstChild()); // !!

        Assert.IsTrue(nav.MoveToNext());
        Assert.AreEqual("item2b", nav.Name);
        Assert.AreEqual(string.Empty, nav.Value);
        Assert.IsFalse(nav.MoveToFirstChild()); // !!

        // we have no way to tell the navigable that a value is CDATA
        // so the rule is, if it's null it's not there, anything else is there
        // and the filtering has to be done when building the content
        Assert.IsTrue(nav.MoveToNext());
        Assert.AreEqual("item2c", nav.Name);
        Assert.AreEqual("\n        ", nav.Value.Lf()); // ok since it's a property
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(XPathNodeType.Text, nav.NodeType);
        Assert.AreEqual(string.Empty, nav.Name);
        Assert.AreEqual("\n        ", nav.Value.Lf());
        Assert.IsTrue(nav.MoveToParent());

        Assert.IsTrue(nav.MoveToNext());
        Assert.AreEqual("item3", nav.Name);
        Assert.AreEqual("blah", nav.Value); // ok since it's a property
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(XPathNodeType.Text, nav.NodeType);
        Assert.AreEqual(string.Empty, nav.Name);
        Assert.AreEqual("blah", nav.Value);
        Assert.IsTrue(nav.MoveToParent());

        Assert.IsTrue(nav.MoveToNext());
        Assert.AreEqual("item3a", nav.Name);
        Assert.AreEqual("\n            blah\n        ", nav.Value.Lf()); // ok since it's a property
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(XPathNodeType.Text, nav.NodeType);
        Assert.AreEqual(string.Empty, nav.Name);
        Assert.AreEqual("\n            blah\n        ", nav.Value.Lf());
        Assert.IsTrue(nav.MoveToParent());

        Assert.IsTrue(nav.MoveToNext());
        Assert.AreEqual("item4", nav.Name);
        Assert.AreEqual("bam", nav.Value); // ok since it's a property
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual("subitem", nav.Name);
        Assert.AreEqual("bam", nav.Value); // ok since we're in a fragment
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(XPathNodeType.Text, nav.NodeType);
        Assert.AreEqual(string.Empty, nav.Name);
        Assert.AreEqual("bam", nav.Value);
        Assert.IsFalse(nav.MoveToNext());
        Assert.IsTrue(nav.MoveToParent());
        Assert.AreEqual("subitem", nav.Name);
        Assert.IsFalse(nav.MoveToNext());
        Assert.IsTrue(nav.MoveToParent());
        Assert.AreEqual("item4", nav.Name);

        Assert.IsTrue(nav.MoveToNext());
        Assert.AreEqual("item5", nav.Name);
        Assert.AreEqual("\n        ", nav.Value.Lf());
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(XPathNodeType.Text, nav.NodeType);
        Assert.AreEqual("\n        ", nav.Value.Lf());
    }

    [Test]
    public void Navigate()
    {
        var source = new TestSource1();
        var nav = new NavigableNavigator(source);

        nav.MoveToRoot();
        Assert.AreEqual(string.Empty, nav.Name); // because we're at root
        nav.MoveToFirstChild();
        Assert.AreEqual("root", nav.Name);
        nav.MoveToFirstChild();
        Assert.AreEqual("type1", nav.Name); // our first content
        nav.MoveToFirstAttribute();
        Assert.AreEqual("id", nav.Name);
        Assert.AreEqual("1", nav.Value);
        nav.MoveToNextAttribute();
        Assert.AreEqual("prop1", nav.Name);
        Assert.AreEqual("1:p1", nav.Value);
        nav.MoveToNextAttribute();
        Assert.AreEqual("prop2", nav.Name);
        Assert.AreEqual("1:p2", nav.Value);
        Assert.IsFalse(nav.MoveToNextAttribute());
        nav.MoveToParent();
        nav.MoveToFirstChild();
        Assert.AreEqual("prop3", nav.Name);
        Assert.AreEqual("1:p3", nav.Value);

        Assert.IsFalse(nav.MoveToNext());
    }

    [Test]
    public void NavigateMixed()
    {
        var source = new TestSource2();
        var nav = new NavigableNavigator(source);

        nav.MoveToRoot();
        nav.MoveToFirstChild();
        Assert.AreEqual("root", nav.Name);
        nav.MoveToFirstChild();
        Assert.AreEqual("type1", nav.Name); // our content
        nav.MoveToFirstChild();
        Assert.AreEqual("prop1", nav.Name); // our property
        Assert.AreEqual(XPathNodeType.Element, nav.NodeType);
        nav.MoveToFirstChild();

        // "<data><item1>poo</item1><item2 xx=\"33\" /><item2 xx=\"34\" /></data>"
        Assert.AreEqual(XPathNodeType.Element, nav.NodeType);
        Assert.AreEqual("data", nav.Name);

        nav.MoveToFirstChild();
        Assert.AreEqual(XPathNodeType.Element, nav.NodeType);
        Assert.AreEqual("item1", nav.Name);

        nav.MoveToNext();
        Assert.AreEqual(XPathNodeType.Element, nav.NodeType);
        Assert.AreEqual("item2", nav.Name);

        nav.MoveToParent();
        Assert.AreEqual(XPathNodeType.Element, nav.NodeType);
        Assert.AreEqual("data", nav.Name);

        nav.MoveToParent();
        Assert.AreEqual(XPathNodeType.Element, nav.NodeType);
        Assert.AreEqual("prop1", nav.Name);
    }

    [Test]
    public void OuterXmlBasic()
    {
        const string xml = @"<root id=""-1"" />";

        var doc = XmlHelper.CreateXPathDocument(xml);
        var nnav = doc.CreateNavigator();
        Assert.AreEqual(xml, nnav.OuterXml);

        var source = new TestSource0();
        var nav = new NavigableNavigator(source);
        Assert.AreEqual(xml, nav.OuterXml);
    }

    [Test]
    public void OuterXml()
    {
        var source = new TestSource1();
        var nav = new NavigableNavigator(source);

        const string xml = @"<root id=""-1"" prop1="""" prop2="""">
  <type1 id=""1"" prop1=""1:p1"" prop2=""1:p2"">
    <prop3>1:p3</prop3>
  </type1>
</root>";

        Assert.AreEqual(xml.Lf(), nav.OuterXml.Lf());
    }

    [Test]
    public void OuterXmlMixed()
    {
        var source = new TestSource2();
        var nav = new NavigableNavigator(source);

        nav.MoveToRoot();

        const string outerXml = @"<root id=""-1"">
  <type1 id=""1"">
    <prop1>
      <data>
        <item1>poo</item1>
        <item2 xx=""33"" />
        <item2 xx=""34"" />
      </data>
    </prop1>
  </type1>
</root>";

        Assert.AreEqual(outerXml.Lf(), nav.OuterXml.Lf());
    }

    [Test]
    public void Query()
    {
        var source = new TestSource1();
        var nav = new NavigableNavigator(source);

        var iterator = nav.Select("//type1");
        Assert.AreEqual(1, iterator.Count);
        iterator.MoveNext();
        Assert.AreEqual("type1", iterator.Current.Name);

        iterator = nav.Select("//* [@prop1='1:p1']");
        Assert.AreEqual(1, iterator.Count);
        iterator.MoveNext();
        Assert.AreEqual("type1", iterator.Current.Name);
    }

    [Test]
    public void QueryMixed()
    {
        var source = new TestSource2();
        var nav = new NavigableNavigator(source);

        var doc = XmlHelper.CreateXPathDocument(
            "<data><item1>poo</item1><item2 xx=\"33\" /><item2 xx=\"34\" /></data>");
        var docNav = doc.CreateNavigator();
        var docIter = docNav.Select("//item2 [@xx=33]");
        Assert.AreEqual(1, docIter.Count);
        Assert.AreEqual(string.Empty, docIter.Current.Name);
        docIter.MoveNext();
        Assert.AreEqual("item2", docIter.Current.Name);

        var iterator = nav.Select("//item2 [@xx=33]");
        Assert.AreEqual(1, iterator.Count);
        Assert.AreEqual(string.Empty, iterator.Current.Name);
        iterator.MoveNext();
        Assert.AreEqual("item2", iterator.Current.Name);
    }

    [Test]
    public void QueryWithVariables()
    {
        var source = new TestSource1();
        var nav = new NavigableNavigator(source);

        var iterator = nav.Select("//* [@prop1=$var]", new XPathVariable("var", "1:p1"));
        Assert.AreEqual(1, iterator.Count);
        iterator.MoveNext();
        Assert.AreEqual("type1", iterator.Current.Name);
    }

    [Test]
    public void QueryMixedWithVariables()
    {
        var source = new TestSource2();
        var nav = new NavigableNavigator(source);

        var iterator = nav.Select("//item2 [@xx=$var]", new XPathVariable("var", "33"));
        Assert.AreEqual(1, iterator.Count);
        iterator.MoveNext();
        Assert.AreEqual("item2", iterator.Current.Name);
    }

    [Test]
    public void MixedWithNoValue()
    {
        var source = new TestSource4();
        var nav = new NavigableNavigator(source);

        var doc = XmlHelper.CreateXPathDocument(@"<root id=""-1"">
                        <type1 id=""1""><prop1><data value=""value""/></prop1><prop2>dang</prop2></type1>
                        <type1 id=""2""><prop1 /><prop2></prop2></type1>
                        <type1 id=""3""><prop1 /><prop2 /></type1>
                    </root>");
        var docNav = doc.CreateNavigator();

        docNav.MoveToRoot();
        Assert.IsTrue(docNav.MoveToFirstChild());
        Assert.AreEqual("root", docNav.Name);
        Assert.IsTrue(docNav.MoveToFirstChild());
        Assert.AreEqual("type1", docNav.Name);
        Assert.IsTrue(docNav.MoveToNext());
        Assert.AreEqual("type1", docNav.Name);
        Assert.IsTrue(docNav.MoveToNext());
        Assert.AreEqual("type1", docNav.Name);
        Assert.IsFalse(docNav.MoveToNext());

        docNav.MoveToRoot();
        var docOuter = docNav.OuterXml;

        nav.MoveToRoot();
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual("root", nav.Name);
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual("type1", nav.Name);
        Assert.IsTrue(nav.MoveToNext());
        Assert.AreEqual("type1", nav.Name);
        Assert.IsTrue(nav.MoveToNext());
        Assert.AreEqual("type1", nav.Name);
        Assert.IsFalse(nav.MoveToNext());

        nav.MoveToRoot();
        var outer = nav.OuterXml;

        Assert.AreEqual(docOuter, outer);
    }

    [Test]
    [Ignore("NavigableNavigator does not implement IHasXmlNode.")]
    public void XmlNodeList()
    {
        var source = new TestSource1();
        var nav = new NavigableNavigator(source);

        var iterator = nav.Select("/*");

        // but, that requires that the underlying navigator implements IHasXmlNode
        // so it is possible to obtain nodes from the navigator - not possible yet
        var nodes = XmlNodeListFactory.CreateNodeList(iterator);

        Assert.AreEqual(nodes.Count, 1);
        var node = nodes[0];

        Assert.AreEqual(3, node.Attributes.Count);
        Assert.AreEqual("1", node.Attributes["id"].Value);
        Assert.AreEqual("1:p1", node.Attributes["prop1"].Value);
        Assert.AreEqual("1:p2", node.Attributes["prop2"].Value);
        Assert.AreEqual(1, node.ChildNodes.Count);
        Assert.AreEqual("prop3", node.FirstChild.Name);
        Assert.AreEqual("1:p3", node.FirstChild.Value);
    }

    [Test]
    public void CloneIsSafe()
    {
        var source = new TestSource5();
        var nav = new NavigableNavigator(source);

        Assert.AreEqual(NavigableNavigator.StatePosition.Root, nav.InternalState.Position);
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual("root", nav.Name); // at -1
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(1, (nav.UnderlyingObject as TestContent).Id);
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(NavigableNavigator.StatePosition.PropertyElement, nav.InternalState.Position);
        Assert.AreEqual("prop1", nav.Name);
        Assert.IsTrue(nav.MoveToNext());
        Assert.AreEqual(NavigableNavigator.StatePosition.PropertyElement, nav.InternalState.Position);
        Assert.AreEqual("prop2", nav.Name);
        Assert.IsTrue(nav.MoveToNext());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(3, (nav.UnderlyingObject as TestContent).Id);

        // at that point nav is at /root/1/3
        var clone = nav.Clone() as NavigableNavigator;

        // move nav to /root/1/5 and ensure that clone stays at /root/1/3
        Assert.IsTrue(nav.MoveToNext());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(5, (nav.UnderlyingObject as TestContent).Id);
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, clone.InternalState.Position);
        Assert.AreEqual(3, (clone.UnderlyingObject as TestContent).Id);

        // move nav to /root/2
        Assert.IsTrue(nav.MoveToParent());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(1, (nav.UnderlyingObject as TestContent).Id);
        Assert.IsTrue(nav.MoveToNext());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(2, (nav.UnderlyingObject as TestContent).Id);
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(NavigableNavigator.StatePosition.PropertyElement, nav.InternalState.Position);
        Assert.AreEqual("prop1", nav.Name);
        Assert.AreEqual("p21", nav.Value);

        // move clone to .. /root/1
        Assert.IsTrue(clone.MoveToParent());

        // clone has not been corrupted by nav
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, clone.InternalState.Position);
        Assert.AreEqual(1, (clone.UnderlyingObject as TestContent).Id);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    public void SelectById(int id)
    {
        var source = new TestSource5();
        var nav = new NavigableNavigator(source);

        var iter = nav.Select(string.Format("//* [@id={0}]", id));
        Assert.IsTrue(iter.MoveNext());
        var current = iter.Current as NavigableNavigator;
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, current.InternalState.Position);
        Assert.AreEqual(id, (current.UnderlyingObject as TestContent).Id);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    public void SelectByIdWithVariable(int id)
    {
        var source = new TestSource5();
        var nav = new NavigableNavigator(source);

        var iter = nav.Select("//* [@id=$id]", new XPathVariable("id", id.ToString(CultureInfo.InvariantCulture)));
        Assert.IsTrue(iter.MoveNext());
        var current = iter.Current as NavigableNavigator;
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, current.InternalState.Position);
        Assert.AreEqual(id, (current.UnderlyingObject as TestContent).Id);
    }

    [Test]
    public void MoveToId()
    {
        var source = new TestSource5();
        var nav = new NavigableNavigator(source);

        // move to /root/1/3
        Assert.IsTrue(nav.MoveToId("3"));
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(3, (nav.UnderlyingObject as TestContent).Id);

        // move to /root/1
        Assert.IsTrue(nav.MoveToParent());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(1, (nav.UnderlyingObject as TestContent).Id);

        // move to /root
        Assert.IsTrue(nav.MoveToParent());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(-1, (nav.UnderlyingObject as TestContent).Id);

        // move up
        Assert.IsTrue(nav.MoveToParent());
        Assert.AreEqual(NavigableNavigator.StatePosition.Root, nav.InternalState.Position);
        Assert.IsFalse(nav.MoveToParent());

        // move to /root/1
        Assert.IsTrue(nav.MoveToId("1"));
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(1, (nav.UnderlyingObject as TestContent).Id);

        // move to /root
        Assert.IsTrue(nav.MoveToParent());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(-1, (nav.UnderlyingObject as TestContent).Id);

        // move up
        Assert.IsTrue(nav.MoveToParent());
        Assert.AreEqual(NavigableNavigator.StatePosition.Root, nav.InternalState.Position);
        Assert.IsFalse(nav.MoveToParent());

        // move to /root
        Assert.IsTrue(nav.MoveToId("-1"));
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(-1, (nav.UnderlyingObject as TestContent).Id);

        // move up
        Assert.IsTrue(nav.MoveToParent());
        Assert.AreEqual(NavigableNavigator.StatePosition.Root, nav.InternalState.Position);
        Assert.IsFalse(nav.MoveToParent());

        // move to /root
        Assert.IsTrue(nav.MoveToId("-1"));
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(-1, (nav.UnderlyingObject as TestContent).Id);

        // move down
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(1, (nav.UnderlyingObject as TestContent).Id);

        // get lost
        Assert.IsFalse(nav.MoveToId("666"));
    }

    [Test]
    public void RootedNavigator1()
    {
        var source = new TestSource5();
        var nav = new NavigableNavigator(source, 1);

        // go to (/root) /1
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(1, (nav.UnderlyingObject as TestContent).Id);

        // go to (/root) /1/prop1
        Assert.IsTrue(nav.MoveToFirstChild());

        // go to (/root) /1/prop2
        Assert.IsTrue(nav.MoveToNext());

        // go to (/root) /1/3
        Assert.IsTrue(nav.MoveToNext());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(3, (nav.UnderlyingObject as TestContent).Id);

        // go to (/root) /1
        Assert.IsTrue(nav.MoveToParent());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(1, (nav.UnderlyingObject as TestContent).Id);

        // go to (/root) ie root
        Assert.IsTrue(nav.MoveToParent());
        Assert.AreEqual(NavigableNavigator.StatePosition.Root, nav.InternalState.Position);
        Assert.IsFalse(nav.MoveToParent());

        // can't go there
        Assert.IsFalse(nav.MoveToId("2"));
    }

    [Test]
    public void RootedNavigator2()
    {
        var source = new TestSource5();
        var nav = new NavigableNavigator(source, 3);

        // go to (/root/1) /3
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(3, (nav.UnderlyingObject as TestContent).Id);

        // go to (/root/1)
        Assert.IsTrue(nav.MoveToParent());
        Assert.AreEqual(NavigableNavigator.StatePosition.Root, nav.InternalState.Position);
        Assert.IsFalse(nav.MoveToParent());

        // can't go there
        Assert.IsFalse(nav.MoveToId("1"));
    }

    [Test]
    public void RootedMaxDepthNavigator()
    {
        var source = new TestSource5();
        var nav = new NavigableNavigator(source, 1, 0);

        // go to (/root) /1
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(1, (nav.UnderlyingObject as TestContent).Id);

        // go to (/root)
        Assert.IsTrue(nav.MoveToParent());
        Assert.AreEqual(NavigableNavigator.StatePosition.Root, nav.InternalState.Position);
        Assert.IsFalse(nav.MoveToParent());

        // go to (/root) /1
        Assert.IsTrue(nav.MoveToId("1"));
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(1, (nav.UnderlyingObject as TestContent).Id);

        // can't go there
        Assert.IsFalse(nav.MoveToId("-1"));
        Assert.IsFalse(nav.MoveToId("3"));
    }

    [Test]
    public void MaxDepthNavigator()
    {
        var source = new TestSource5();
        var nav = new NavigableNavigator(source, maxDepth: 1);

        // go to /root
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(-1, (nav.UnderlyingObject as TestContent).Id);

        // go to /root/1
        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(1, (nav.UnderlyingObject as TestContent).Id);
        Assert.IsTrue(nav.MoveToId("1"));
        Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        Assert.AreEqual(1, (nav.UnderlyingObject as TestContent).Id);

        // go to /root/1/prop1
        Assert.IsTrue(nav.MoveToFirstChild());

        // go to /root/1/prop2
        Assert.IsTrue(nav.MoveToNext());

        // can't go to /root/1/3
        Assert.IsFalse(nav.MoveToNext());
        Assert.IsFalse(nav.MoveToId("3"));

        //// Assert.AreEqual(NavigableNavigator.StatePosition.Element, nav.InternalState.Position);
        //// Assert.AreEqual(3, (nav.UnderlyingObject as TestContent).Id);
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void XsltDebugModeAndSortOrder(bool native, bool debug)
    {
        const string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <node id=""1"" isDoc=""1"">
        <title>title-1</title>
        <node id=""3"" isDoc=""1"">
            <title>title-3</title>
            <node id=""7"" isDoc=""1"">
                <title>title-7</title>
            </node>
            <node id=""8"" isDoc=""1"">
                <title>title-8</title>
            </node>
        </node>
        <node id=""5"" isDoc=""1"">
            <title>title-5</title>
        </node>
    </node>
    <node id=""2"" isDoc=""1"">
        <title>title-2</title>
        <node id=""4"" isDoc=""1"">
            <title>title-4</title>
        </node>
        <node id=""6"" isDoc=""1"">
            <title>title-6</title>
        </node>
    </node>
</root>
";

        const string xslt = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE xsl:stylesheet [
    <!ENTITY nbsp ""&#x00A0;"">
]>
<xsl:stylesheet
  version=""1.0""
  xmlns:xsl=""http://www.w3.org/1999/XSL/Transform""
  xmlns:msxml=""urn:schemas-microsoft-com:xslt""
    xmlns:umbraco.library=""urn:umbraco.library"" xmlns:Exslt.ExsltCommon=""urn:Exslt.ExsltCommon"" xmlns:Exslt.ExsltDatesAndTimes=""urn:Exslt.ExsltDatesAndTimes"" xmlns:Exslt.ExsltMath=""urn:Exslt.ExsltMath"" xmlns:Exslt.ExsltRegularExpressions=""urn:Exslt.ExsltRegularExpressions"" xmlns:Exslt.ExsltStrings=""urn:Exslt.ExsltStrings"" xmlns:Exslt.ExsltSets=""urn:Exslt.ExsltSets"" xmlns:Examine=""urn:Examine""
    exclude-result-prefixes=""msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets Examine "">

    <xsl:output method=""xml"" omit-xml-declaration=""yes"" />
    <xsl:param name=""currentPage""/>

    <xsl:template match=""/"">
        <!-- <xsl:for-each select=""/root/* [@isDoc]""> -->
        <!-- <xsl:for-each select=""$currentPage/root/* [@isDoc]""> -->
        <xsl:for-each select=""/macro/nav/root/* [@isDoc]"">
<xsl:text>! </xsl:text><xsl:value-of select=""title"" /><xsl:text>
</xsl:text>
            <xsl:for-each select=""./* [@isDoc]"">
<xsl:text>!! </xsl:text><xsl:value-of select=""title"" /><xsl:text>
</xsl:text>
                <xsl:for-each select=""./* [@isDoc]"">
<xsl:text>!!! </xsl:text><xsl:value-of select=""title"" /><xsl:text>
</xsl:text>
                </xsl:for-each>
            </xsl:for-each>
        </xsl:for-each>
    </xsl:template>

</xsl:stylesheet>
";
        const string expected = @"! title-1
!! title-3
!!! title-7
!!! title-8
!! title-5
! title-2
!! title-4
!! title-6
";

        // see http://www.onenaught.com/posts/352/xslt-performance-tip-dont-indent-output
        // why aren't we using an XmlWriter here?
        var transform = new XslCompiledTransform(debug);
        var xmlReader = new XmlTextReader(new StringReader(xslt)) { EntityHandling = EntityHandling.ExpandEntities };
        var xslResolver = new XmlUrlResolver { Credentials = CredentialCache.DefaultCredentials };
        var args = new XsltArgumentList();

        // .Default is more restrictive than .TrustedXslt
        transform.Load(xmlReader, XsltSettings.Default, xslResolver);

        XPathNavigator macro;
        if (!native)
        {
            var source = new TestSource7();
            ////var nav = new NavigableNavigator(source);
            ////args.AddParam("currentPage", string.Empty, nav.Clone());

            var x = new XmlDocument();
            x.LoadXml(xml);

            macro = new MacroNavigator(new[]
            {
                // it even fails like that => macro nav. issue?
                new MacroNavigator.MacroParameter("nav", x.CreateNavigator()), // nav.Clone())
            });
        }
        else
        {
            var doc = new XmlDocument();
            doc.LoadXml("<macro />");
            var nav = doc.CreateElement("nav");
            doc.DocumentElement.AppendChild(nav);
            var x = new XmlDocument();
            x.LoadXml(xml);
            nav.AppendChild(doc.ImportNode(x.DocumentElement, true));
            macro = doc.CreateNavigator();
        }

        var writer = new StringWriter();
        transform.Transform(macro, args, writer);

        // this was working with native, debug and non-debug
        // this was working with macro nav, non-debug
        // but was NOT working (changing the order of nodes) with macro nav, debug
        // was due to an issue with macro nav IsSamePosition, fixed

        ////Debug.Print("--------");
        ////Debug.Print(writer.ToString());
        Assert.AreEqual(expected.Lf(), writer.ToString().Lf());
    }

    [Test]
    public void WhiteSpacesAndEmptyValues()
    {
        // "When Microsoft’s DOM builder receives a text node from the parser
        // that contains only white space, it is thrown away." - so if it's ONLY
        // spaces, it's nothing, but spaces are NOT trimmed.

        // For attributes, spaces are preserved even when there's only spaces.
        var doc = XmlHelper.CreateXPathDocument(@"<root>
                        <item><prop/></item>
                        <item><prop></prop></item>
                        <item><prop> </prop></item>
                        <item><prop>
                            </prop></item>
                        <item><prop> ooo </prop></item>
                        <item><prop> ooo
                            </prop></item>
                        <item x=""""/>
                        <item x="" ""/>
                    </root>");

        var docNav = doc.CreateNavigator();

        Assert.AreEqual(
            @"<root>
  <item>
    <prop />
  </item>
  <item>
    <prop></prop>
  </item>
  <item>
    <prop></prop>
  </item>
  <item>
    <prop></prop>
  </item>
  <item>
    <prop> ooo </prop>
  </item>
  <item>
    <prop> ooo
                            </prop>
  </item>
  <item x="""" />
  <item x="" "" />
</root>".Lf(),
            docNav.OuterXml.Lf());

        docNav.MoveToRoot();
        Assert.IsTrue(docNav.MoveToFirstChild());
        Assert.IsTrue(docNav.MoveToFirstChild());
        Assert.IsTrue(docNav.MoveToFirstChild()); // prop
        Assert.IsTrue(docNav.IsEmptyElement);
        Assert.IsTrue(docNav.MoveToParent());
        Assert.IsTrue(docNav.MoveToNext());
        Assert.IsTrue(docNav.MoveToFirstChild()); // prop
        Assert.IsFalse(docNav.IsEmptyElement);
        Assert.AreEqual(string.Empty, docNav.Value); // contains an empty text node
        Assert.IsTrue(docNav.MoveToParent());
        Assert.IsTrue(docNav.MoveToNext());
        Assert.IsTrue(docNav.MoveToFirstChild()); // prop
        Assert.IsFalse(docNav.IsEmptyElement);
        Assert.AreEqual(string.Empty, docNav.Value); // contains an empty text node

        var source = new TestSource8();
        var nav = new NavigableNavigator(source);

        // shows how whitespaces are handled by NavigableNavigator
        Assert.AreEqual(
            @"<root id=""-1"" attr="""">
  <item id=""1"" attr="""">
    <prop />
  </item>
  <item id=""2"" attr="""">
    <prop></prop>
  </item>
  <item id=""3"" attr=""   "">
    <prop>   </prop>
  </item>
  <item id=""4"" attr="""">
    <prop>
</prop>
  </item>
  <item id=""5"" attr=""  ooo  "">
    <prop>   ooo   </prop>
  </item>
</root>".Lf(),
            nav.OuterXml.Lf());
    }
}

internal class TestPropertyType : INavigableFieldType
{
    public TestPropertyType(string name, bool isXmlContent = false, Func<object, string> xmlStringConverter = null)
    {
        Name = name;
        IsXmlContent = isXmlContent;
        XmlStringConverter = xmlStringConverter;
    }

    public bool IsXmlContent { get; }

    public string Name { get; }

    public Func<object, string> XmlStringConverter { get; }
}

internal class TestContentType : INavigableContentType
{
    public TestContentType(TestSourceBase source, string name, params INavigableFieldType[] properties)
    {
        Source = source;
        Name = name;
        FieldTypes = properties;
    }

    public TestSourceBase Source { get; }

    public string Name { get; }

    public INavigableFieldType[] FieldTypes { get; protected set; }
}

internal class TestRootContentType : TestContentType
{
    public TestRootContentType(TestSourceBase source, params INavigableFieldType[] properties)
        : base(source, "root") => FieldTypes = properties;

    public TestContentType CreateType(string name, params INavigableFieldType[] properties) =>
        new(Source, name, FieldTypes.Union(properties).ToArray());
}

internal class TestContent : INavigableContent
{
    private readonly TestContentType _type;

    public TestContent(TestContentType type, int id, int parentId)
    {
        _type = type;
        Id = id;
        ParentId = parentId;
    }

    // locals
    public object[] FieldValues { get; private set; }

    public int Id { get; }

    public int ParentId { get; }

    public INavigableContentType Type => _type;

    public IList<int> ChildIds { get; private set; }

    public object Value(int id)
    {
        if (!(_type.FieldTypes[id] is TestPropertyType fieldType))
        {
            throw new Exception("Oops");
        }

        var value = FieldValues[id];
        var isAttr = id <= _type.Source.LastAttributeIndex;

        // null => return null
        if (value == null)
        {
            return null;
        }

        // attribute => return string value
        if (isAttr)
        {
            return value.ToString();
        }

        // has a converter => use the converter
        if (fieldType.XmlStringConverter != null)
        {
            return fieldType.XmlStringConverter(value);
        }

        // not a string => return value as a string
        if (!(value is string s))
        {
            return value.ToString();
        }

        // xml content... try xml
        if (fieldType.IsXmlContent)
        {
            if (XmlHelper.TryCreateXPathDocumentFromPropertyValue(s, out var doc))
            {
                return doc.CreateNavigator();
            }
        }

        // return the string
        // even if it's xml that can't be parsed...
        return s;
    }

    public TestContent WithChildren(params int[] childIds)
    {
        ChildIds = childIds;
        return this;
    }

    public TestContent WithValues(params object[] values)
    {
        FieldValues = values ?? new object[] { null };
        return this;
    }
}

internal class TestRootContent : TestContent
{
    public TestRootContent(TestContentType type)
        : base(type, -1, -1)
    {
    }
}

internal abstract class TestSourceBase : INavigableSource
{
    protected readonly Dictionary<int, TestContent> Content = new();

    public INavigableContent Get(int id) => Content.ContainsKey(id) ? Content[id] : null;

    public int LastAttributeIndex { get; protected set; }

    public INavigableContent Root { get; protected set; }
}

internal class TestSource0 : TestSourceBase
{
    public TestSource0()
    {
        LastAttributeIndex = -1;
        var type = new TestRootContentType(this);
        Root = new TestRootContent(type);
    }
}

internal class TestSource1 : TestSourceBase
{
    public TestSource1()
    {
        // last attribute index is 1 -  meaning properties 0 and 1 are attributes, 2+ are elements
        // then, fieldValues must have adequate number of items
        LastAttributeIndex = 1;

        var prop1 = new TestPropertyType("prop1");
        var prop2 = new TestPropertyType("prop2");
        var prop3 = new TestPropertyType("prop3");
        var type = new TestRootContentType(this, prop1, prop2);
        var type1 = type.CreateType("type1", prop3);

        Content[1] = new TestContent(type1, 1, -1).WithValues("1:p1", "1:p2", "1:p3");

        Root = new TestRootContent(type).WithValues(string.Empty, string.Empty).WithChildren(1);
    }
}

internal class TestSource2 : TestSourceBase
{
    public TestSource2()
    {
        LastAttributeIndex = -1;

        var prop1 = new TestPropertyType("prop1", true);
        var type = new TestRootContentType(this);
        var type1 = type.CreateType("type1", prop1);

        const string xml = "<data><item1>poo</item1><item2 xx=\"33\" /><item2 xx=\"34\" /></data>";
        Content[1] = new TestContent(type1, 1, 1).WithValues(xml);

        Root = new TestRootContent(type).WithChildren(1);
    }
}

internal class TestSource3 : TestSourceBase
{
    public TestSource3()
    {
        LastAttributeIndex = 1;

        var prop1 = new TestPropertyType("prop1");
        var prop2 = new TestPropertyType("prop2");
        var prop3 = new TestPropertyType("prop3");
        var type = new TestRootContentType(this, prop1, prop2);
        var type1 = type.CreateType("type1", prop3);

        Content[1] = new TestContent(type1, 1, 1).WithValues("1:p1", "1:p2", "1:p3").WithChildren(2);
        Content[2] = new TestContent(type1, 2, 1).WithValues("2:p1", "2:p2", "2:p3");

        Root = new TestRootContent(type).WithChildren(1);
    }
}

internal class TestSource4 : TestSourceBase
{
    public TestSource4()
    {
        LastAttributeIndex = -1;

        var prop1 = new TestPropertyType("prop1", true);
        var prop2 = new TestPropertyType("prop2");
        var type = new TestRootContentType(this);
        var type1 = type.CreateType("type1", prop1, prop2);

        Content[1] = new TestContent(type1, 1, -1).WithValues("<data value=\"value\"/>", "dang");
        Content[2] = new TestContent(type1, 2, -1).WithValues(null, string.Empty);
        Content[3] = new TestContent(type1, 3, -1).WithValues(null, null);

        Root = new TestRootContent(type).WithChildren(1, 2, 3);
    }
}

internal class TestSource5 : TestSourceBase
{
    public TestSource5()
    {
        LastAttributeIndex = -1;

        var prop1 = new TestPropertyType("prop1");
        var prop2 = new TestPropertyType("prop2");
        var type = new TestRootContentType(this);
        var type1 = type.CreateType("type1", prop1, prop2);

        Content[1] = new TestContent(type1, 1, -1).WithValues("p11", "p12").WithChildren(3, 5);
        Content[2] = new TestContent(type1, 2, -1).WithValues("p21", "p22").WithChildren(4, 6);
        Content[3] = new TestContent(type1, 3, 1).WithValues("p31", "p32");
        Content[4] = new TestContent(type1, 4, 2).WithValues("p41", "p42");
        Content[5] = new TestContent(type1, 5, 1).WithValues("p51", "p52");
        Content[6] = new TestContent(type1, 6, 2).WithValues("p61", "p62");

        Root = new TestRootContent(type).WithChildren(1, 2);
    }
}

internal class TestSource6 : TestSourceBase
{
    ////<root>
    ////    <wrap>
    ////        <item1 />
    ////        <item2></item2>
    ////        <item2a> </item2a>
    ////        <item2b>
    ////        </item2b>
    ////        <item2c><![CDATA[
    ////        ]]></item2c>
    ////        <item3>blah</item3>
    ////        <item4>
    ////            <subitem x=""1"">bam</subitem>
    ////        </item4>
    ////        <item5>
    ////        </item5>
    ////    </wrap>
    ////</root>

    public TestSource6()
    {
        LastAttributeIndex = -1;

        var type = new TestRootContentType(this);
        var type1 = type.CreateType(
            "wrap",
            new TestPropertyType("item1"),
            new TestPropertyType("item2"),
            new TestPropertyType("item2a"),
            new TestPropertyType("item2b"),
            new TestPropertyType("item2c"),
            new TestPropertyType("item3"),
            new TestPropertyType("item3a"),
            new TestPropertyType("item4", true),
            new TestPropertyType("item5", true));

        Content[1] = new TestContent(type1, 1, -1)
            .WithValues(
                null,
                null,
                null,
                null,
                "\n        ",
                "blah",
                "\n            blah\n        ",
                "<subitem x=\"1\">bam</subitem>",
                "\n        ");

        Root = new TestRootContent(type).WithChildren(1);
    }
}

internal class TestSource7 : TestSourceBase
{
    public TestSource7()
    {
        LastAttributeIndex = 0;

        var prop1 = new TestPropertyType("isDoc");
        var prop2 = new TestPropertyType("title");
        var type = new TestRootContentType(this, prop1);
        var type1 = type.CreateType("node", prop1, prop2);

        Content[1] = new TestContent(type1, 1, -1).WithValues(1, "title-1").WithChildren(3, 5);
        Content[2] = new TestContent(type1, 2, -1).WithValues(1, "title-2").WithChildren(4, 6);
        Content[3] = new TestContent(type1, 3, 1).WithValues(1, "title-3").WithChildren(7, 8);
        Content[4] = new TestContent(type1, 4, 2).WithValues(1, "title-4");
        Content[5] = new TestContent(type1, 5, 1).WithValues(1, "title-5");
        Content[6] = new TestContent(type1, 6, 2).WithValues(1, "title-6");

        Content[7] = new TestContent(type1, 7, 3).WithValues(1, "title-7");
        Content[8] = new TestContent(type1, 8, 3).WithValues(1, "title-8");

        Root = new TestRootContent(type).WithValues(null).WithChildren(1, 2);
    }
}

internal class TestSource8 : TestSourceBase
{
    public TestSource8()
    {
        LastAttributeIndex = 0;

        var attr = new TestPropertyType("attr");
        var prop = new TestPropertyType("prop");
        var type = new TestRootContentType(this, attr);
        var type1 = type.CreateType("item", attr, prop);
        Content[1] = new TestContent(type1, 1, -1).WithValues(null, null);
        Content[2] = new TestContent(type1, 2, -1).WithValues(string.Empty, string.Empty);
        Content[3] = new TestContent(type1, 3, -1).WithValues("   ", "   ");
        Content[4] = new TestContent(type1, 4, -1).WithValues(string.Empty, "\n");
        Content[5] = new TestContent(type1, 5, -1).WithValues("  ooo  ", "   ooo   ");
        Root = new TestRootContent(type).WithValues(null).WithChildren(1, 2, 3, 4, 5);
    }
}
