using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Xml;
using Umbraco.Core.Xml.XPath;
using NUnit.Framework;

namespace Umbraco.Tests.CoreXml
{
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

        static void NavigatorValues(XPathNavigator nav, bool native)
        {
            // in non-native we can't have Value dump everything, else
            // we'd dump the entire database? Makes not much sense.

            Assert.AreEqual(native ? "\r\n        blah\r\n            blah\r\n        bam\r\n        " : string.Empty, nav.Value); // !!
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.AreEqual("root", nav.Name);
            Assert.AreEqual(native ? "\r\n        blah\r\n            blah\r\n        bam\r\n        " : string.Empty, nav.Value); // !!
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.AreEqual("wrap", nav.Name);
            Assert.AreEqual(native ? "\r\n        blah\r\n            blah\r\n        bam\r\n        " : string.Empty, nav.Value); // !!

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
            Assert.AreEqual("\r\n        ", nav.Value); // ok since it's a property
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.AreEqual(XPathNodeType.Text, nav.NodeType);
            Assert.AreEqual(string.Empty, nav.Name);
            Assert.AreEqual("\r\n        ", nav.Value);
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
            Assert.AreEqual("\r\n            blah\r\n        ", nav.Value); // ok since it's a property
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.AreEqual(XPathNodeType.Text, nav.NodeType);
            Assert.AreEqual(string.Empty, nav.Name);
            Assert.AreEqual("\r\n            blah\r\n        ", nav.Value);
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
            Assert.AreEqual("\r\n        ", nav.Value);
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.AreEqual(XPathNodeType.Text, nav.NodeType);
            Assert.AreEqual("\r\n        ", nav.Value);
        }

        [Test]
        public void Navigate()
        {
            var source = new TestSource1();
            var nav = new NavigableNavigator(source);

            nav.MoveToRoot();
            Assert.AreEqual("", nav.Name); // because we're at root
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

            Assert.AreEqual(xml, nav.OuterXml);
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

            Assert.AreEqual(outerXml, nav.OuterXml);
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

            var doc = XmlHelper.CreateXPathDocument("<data><item1>poo</item1><item2 xx=\"33\" /><item2 xx=\"34\" /></data>");
            var docNav = doc.CreateNavigator();
            var docIter = docNav.Select("//item2 [@xx=33]");
            Assert.AreEqual(1, docIter.Count);
            Assert.AreEqual("", docIter.Current.Name);
            docIter.MoveNext();
            Assert.AreEqual("item2", docIter.Current.Name);

            var iterator = nav.Select("//item2 [@xx=33]");
            Assert.AreEqual(1, iterator.Count);
            Assert.AreEqual("", iterator.Current.Name);
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
            TestContent content;

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

            // get lost
            Assert.IsFalse(nav.MoveToId("666"));
        }

        [Test]
        public void RootedNavigator()
        {
            var source = new TestSource5();
            var nav = new NavigableNavigator(source, source.Get(1));

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

            Assert.AreEqual(@"<root>
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
</root>", docNav.OuterXml);

            docNav.MoveToRoot();
            Assert.IsTrue(docNav.MoveToFirstChild());
            Assert.IsTrue(docNav.MoveToFirstChild());
            Assert.IsTrue(docNav.MoveToFirstChild()); // prop
            Assert.IsTrue(docNav.IsEmptyElement);
            Assert.IsTrue(docNav.MoveToParent());
            Assert.IsTrue(docNav.MoveToNext());
            Assert.IsTrue(docNav.MoveToFirstChild()); // prop
            Assert.IsFalse(docNav.IsEmptyElement);
            Assert.AreEqual("", docNav.Value); // contains an empty text node
            Assert.IsTrue(docNav.MoveToParent());
            Assert.IsTrue(docNav.MoveToNext());
            Assert.IsTrue(docNav.MoveToFirstChild()); // prop
            Assert.IsFalse(docNav.IsEmptyElement);
            Assert.AreEqual("", docNav.Value); // contains an empty text node

            var source = new TestSource8();
            var nav = new NavigableNavigator(source);

            // shows how whitespaces are handled by NavigableNavigator
            Assert.AreEqual(@"<root id=""-1"" attr="""">
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
</root>", nav.OuterXml);
        }
    }

    #region Navigable implementation

    class TestPropertyType : INavigableFieldType
    {
        public TestPropertyType(string name, bool isXmlContent = false, Func<object, string> xmlStringConverter = null)
        {
            Name = name;
            IsXmlContent = isXmlContent;
            XmlStringConverter = xmlStringConverter;
        }

        public string Name { get; private set; }
        public bool IsXmlContent { get; private set; }
        public Func<object, string> XmlStringConverter { get; private set; }
    }

    class TestContentType : INavigableContentType
    {
        public TestContentType(TestSourceBase source, string name, params INavigableFieldType[] properties)
        {
            Source = source;
            Name = name;
            FieldTypes = properties;
        }

        public TestSourceBase Source { get; private set; }
        public string Name { get; private set; }
        public INavigableFieldType[] FieldTypes { get; protected set; }
    }

    class TestRootContentType : TestContentType
    {
        public TestRootContentType(TestSourceBase source, params INavigableFieldType[] properties)
            : base(source, "root")
        {
            FieldTypes = properties;
        }

        public TestContentType CreateType(string name, params INavigableFieldType[] properties)
        {
            return new TestContentType(Source, name, FieldTypes.Union(properties).ToArray());
        }
    }

    class TestContent : INavigableContent
    {
        public TestContent(TestContentType type, int id, int parentId)
        {
            _type = type;
            Id = id;
            ParentId = parentId;
        }

        private readonly TestContentType _type;
        public int Id { get; private set; }
        public int ParentId { get; private set; }
        public INavigableContentType Type { get { return _type; } }
        public IList<int> ChildIds { get; private set; }

        public object Value(int id)
        {
            var fieldType = _type.FieldTypes[id];
            var value = FieldValues[id];
            var isAttr = id <= _type.Source.LastAttributeIndex;

            if (value == null) return null;
            if (isAttr) return value.ToString();

            if (fieldType.XmlStringConverter != null) return fieldType.XmlStringConverter(value);

            // though in reality we should use the converters, which should
            // know whether the property is XML or not, instead of guessing.
            XPathDocument doc;
            if (XmlHelper.TryCreateXPathDocumentFromPropertyValue(value, out doc))
                return doc.CreateNavigator();

            //var s = value.ToString();
            //return XmlHelper.IsXmlWhitespace(s) ? null : s;
            return value.ToString();
        }

        // locals
        public object[] FieldValues { get; private set; }

        public TestContent WithChildren(params int[] childIds)
        {
            ChildIds = childIds;
            return this;
        }

        public TestContent WithValues(params object[] values)
        {
            FieldValues = values == null ? new object[] {null} : values;
            return this;
        }
    }

    class TestRootContent : TestContent
    {
        public TestRootContent(TestContentType type)
            : base(type, -1, -1)
        { }
    }

    abstract class TestSourceBase : INavigableSource
    {
        protected readonly Dictionary<int, TestContent> Content = new Dictionary<int, TestContent>();

        public INavigableContent Get(int id)
        {
            return Content.ContainsKey(id) ? Content[id] : null;
        }

        public int LastAttributeIndex { get; protected set; }

        public INavigableContent Root { get; protected set; }
    }

    #endregion

    #region Navigable sources

    class TestSource0 : TestSourceBase
    {
        public TestSource0()
        {
            LastAttributeIndex = -1;
            var type = new TestRootContentType(this);
            Root = new TestRootContent(type);
        }
    }

    class TestSource1 : TestSourceBase
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

            Root = new TestRootContent(type).WithValues("", "").WithChildren(1);
        }
    }

    class TestSource2 : TestSourceBase
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

    class TestSource3 : TestSourceBase
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

    class TestSource4 : TestSourceBase
    {
        public TestSource4()
        {
            LastAttributeIndex = -1;

            var prop1 = new TestPropertyType("prop1", true);
            var prop2 = new TestPropertyType("prop2");
            var type = new TestRootContentType(this);
            var type1 = type.CreateType("type1", prop1, prop2);

            Content[1] = new TestContent(type1, 1, -1).WithValues("<data value=\"value\"/>", "dang");
            Content[2] = new TestContent(type1, 2, -1).WithValues(null, "");
            Content[3] = new TestContent(type1, 3, -1).WithValues(null, null);

            Root = new TestRootContent(type).WithChildren(1, 2, 3);
        }
    }

    class TestSource5 : TestSourceBase
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

    class TestSource6 : TestSourceBase
    {
        //<root>
        //    <wrap>
        //        <item1 />
        //        <item2></item2>
        //        <item2a> </item2a>
        //        <item2b>
        //        </item2b>
        //        <item2c><![CDATA[
        //        ]]></item2c>
        //        <item3>blah</item3>
        //        <item4>
        //            <subitem x=""1"">bam</subitem>
        //        </item4>
        //        <item5>
        //        </item5>
        //    </wrap>
        //</root>
        
        public TestSource6()
        {
            LastAttributeIndex = -1;

            var type = new TestRootContentType(this);
            var type1 = type.CreateType("wrap", 
                new TestPropertyType("item1"),
                new TestPropertyType("item2"),
                new TestPropertyType("item2a"),
                new TestPropertyType("item2b"),
                new TestPropertyType("item2c"),
                new TestPropertyType("item3"),
                new TestPropertyType("item3a"),
                new TestPropertyType("item4", true),
                new TestPropertyType("item5", true)
            );

            Content[1] = new TestContent(type1, 1, -1)
                .WithValues(
                    null,
                    null, 
                    null, 
                    null, 
                    "\r\n        ", 
                    "blah", 
                    "\r\n            blah\r\n        ",
                    "<subitem x=\"1\">bam</subitem>",
                    "\r\n        "
                );

            Root = new TestRootContent(type).WithChildren(1);
        }
    }

    class TestSource7 : TestSourceBase
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

    class TestSource8 : TestSourceBase
    {
        public TestSource8()
        {
            LastAttributeIndex = 0;

            var attr = new TestPropertyType("attr");
            var prop = new TestPropertyType("prop");
            var type = new TestRootContentType(this, attr);
            var type1 = type.CreateType("item", attr, prop);
            Content[1] = new TestContent(type1, 1, -1).WithValues(null, null);
            Content[2] = new TestContent(type1, 2, -1).WithValues("", "");
            Content[3] = new TestContent(type1, 3, -1).WithValues("   ", "   ");
            Content[4] = new TestContent(type1, 4, -1).WithValues("", "\r\n");
            Content[5] = new TestContent(type1, 5, -1).WithValues("  ooo  ", "   ooo   ");
            Root = new TestRootContent(type).WithValues(null).WithChildren(1, 2, 3, 4, 5);
        }
    }

    #endregion
}
