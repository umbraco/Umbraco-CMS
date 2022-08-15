// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Xml;
using NUnit.Framework;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.CoreXml;

[TestFixture]
public class FrameworkXmlTests
{
    private const string Xml1 = @"<root>
    <items>
        <item1 />
        <item2>
            <item21>text21</item21>
        </item2>
        <item3>
            <item31>text31</item31>
        </item3>
        <item4 />
        <item5 />
        <item6 />
    </items>
</root>";

    // Umbraco : the following test shows that when legacy imports the whole tree in a
    // "contentAll" xslt macro parameter, the entire collection of nodes is cloned ie is
    // duplicated.
    //
    // What is the impact on memory?
    // What happens for non-xslt macros?
    [Test]
    public void ImportNodeClonesImportedNode()
    {
        var doc1 = new XmlDocument();
        doc1.LoadXml(Xml1);

        var node1 = doc1.SelectSingleNode("//item2");
        Assert.IsNotNull(node1);

        var doc2 = new XmlDocument();
        doc2.LoadXml("<nodes />");
        var node2 = doc2.ImportNode(node1, true);
        var root2 = doc2.DocumentElement;
        Assert.IsNotNull(root2);
        root2.AppendChild(node2);

        var node3 = doc2.SelectSingleNode("//item2");

        Assert.AreNotSame(node1, node2); // has been cloned
        Assert.AreSame(node2, node3); // has been appended

        Assert.AreNotSame(node1.FirstChild, node2.FirstChild); // deep clone
    }

    // Umbraco: the CanRemove...NodeAndNavigate tests shows that if the underlying XmlDocument
    // is modified while navigating, then strange situations can be created. For xslt macros,
    // the result depends on what the xslt engine is doing at the moment = unpredictable.
    //
    // What happens for non-xslt macros?
    [Test]
    public void CanRemoveCurrentNodeAndNavigate()
    {
        var doc1 = new XmlDocument();
        doc1.LoadXml(Xml1);
        var nav1 = doc1.CreateNavigator();

        Assert.IsTrue(nav1.MoveToFirstChild());
        Assert.AreEqual("root", nav1.Name);
        Assert.IsTrue(nav1.MoveToFirstChild());
        Assert.AreEqual("items", nav1.Name);
        Assert.IsTrue(nav1.MoveToFirstChild());
        Assert.AreEqual("item1", nav1.Name);
        Assert.IsTrue(nav1.MoveToNext());
        Assert.AreEqual("item2", nav1.Name);

        var node1 = doc1.SelectSingleNode("//item2");
        Assert.IsNotNull(node1);
        var parent1 = node1.ParentNode;
        Assert.IsNotNull(parent1);
        parent1.RemoveChild(node1);

        // navigator now navigates on an isolated fragment
        // that is rooted on the node that was removed
        Assert.AreEqual("item2", nav1.Name);
        Assert.IsFalse(nav1.MoveToPrevious());
        Assert.IsFalse(nav1.MoveToNext());
        Assert.IsFalse(nav1.MoveToParent());

        Assert.IsTrue(nav1.MoveToFirstChild());
        Assert.AreEqual("item21", nav1.Name);
        Assert.IsTrue(nav1.MoveToParent());
        Assert.AreEqual("item2", nav1.Name);

        nav1.MoveToRoot();
        Assert.AreEqual("item2", nav1.Name);
    }

    [Test]
    public void CanRemovePathNodeAndNavigate()
    {
        var doc1 = new XmlDocument();
        doc1.LoadXml(Xml1);
        var nav1 = doc1.CreateNavigator();

        Assert.IsTrue(nav1.MoveToFirstChild());
        Assert.AreEqual("root", nav1.Name);
        Assert.IsTrue(nav1.MoveToFirstChild());
        Assert.AreEqual("items", nav1.Name);
        Assert.IsTrue(nav1.MoveToFirstChild());
        Assert.AreEqual("item1", nav1.Name);
        Assert.IsTrue(nav1.MoveToNext());
        Assert.AreEqual("item2", nav1.Name);
        Assert.IsTrue(nav1.MoveToFirstChild());
        Assert.AreEqual("item21", nav1.Name);

        var node1 = doc1.SelectSingleNode("//item2");
        Assert.IsNotNull(node1);
        var parent1 = node1.ParentNode;
        Assert.IsNotNull(parent1);
        parent1.RemoveChild(node1);

        // navigator now navigates on an isolated fragment
        // that is rooted on the node that was removed
        Assert.AreEqual("item21", nav1.Name);
        Assert.IsTrue(nav1.MoveToParent());
        Assert.AreEqual("item2", nav1.Name);
        Assert.IsFalse(nav1.MoveToPrevious());
        Assert.IsFalse(nav1.MoveToNext());
        Assert.IsFalse(nav1.MoveToParent());

        nav1.MoveToRoot();
        Assert.AreEqual("item2", nav1.Name);
    }

    [Test]
    public void CanRemoveOutOfPathNodeAndNavigate()
    {
        var doc1 = new XmlDocument();
        doc1.LoadXml(Xml1);
        var nav1 = doc1.CreateNavigator();

        Assert.IsTrue(nav1.MoveToFirstChild());
        Assert.AreEqual("root", nav1.Name);
        Assert.IsTrue(nav1.MoveToFirstChild());
        Assert.AreEqual("items", nav1.Name);
        Assert.IsTrue(nav1.MoveToFirstChild());
        Assert.AreEqual("item1", nav1.Name);
        Assert.IsTrue(nav1.MoveToNext());
        Assert.AreEqual("item2", nav1.Name);
        Assert.IsTrue(nav1.MoveToNext());
        Assert.AreEqual("item3", nav1.Name);
        Assert.IsTrue(nav1.MoveToNext());
        Assert.AreEqual("item4", nav1.Name);

        var node1 = doc1.SelectSingleNode("//item2");
        Assert.IsNotNull(node1);
        var parent1 = node1.ParentNode;
        Assert.IsNotNull(parent1);
        parent1.RemoveChild(node1);

        // navigator sees the change
        Assert.AreEqual("item4", nav1.Name);
        Assert.IsTrue(nav1.MoveToPrevious());
        Assert.AreEqual("item3", nav1.Name);
        Assert.IsTrue(nav1.MoveToPrevious());
        Assert.AreEqual("item1", nav1.Name);
    }

    // Umbraco: the following test shows that if the underlying XmlDocument is modified while
    // iterating, then strange situations can be created. For xslt macros, the result depends
    // on what the xslt engine is doing at the moment = unpredictable.
    //
    // What happens for non-xslt macros?
    [Test]
    public void CanRemoveNodeAndIterate()
    {
        var doc1 = new XmlDocument();
        doc1.LoadXml(Xml1);
        var nav1 = doc1.CreateNavigator();

        var iter1 = nav1.Select("//items/*");
        var iter2 = nav1.Select("//items/*");

        Assert.AreEqual(6, iter1.Count);

        var node1 = doc1.SelectSingleNode("//item2");
        Assert.IsNotNull(node1);
        var parent1 = node1.ParentNode;
        Assert.IsNotNull(parent1);
        parent1.RemoveChild(node1);

        // iterator partially sees the change
        Assert.AreEqual(6, iter1.Count); // has been cached, not updated
        Assert.AreEqual(5, iter2.Count); // not calculated yet, correct value

        var count = 0;
        while (iter1.MoveNext())
        {
            count++;
        }

        Assert.AreEqual(5, count);
    }

    [Test]
    public void OldFrameworkXPathBugIsFixed()
    {
        // see http://bytes.com/topic/net/answers/177129-reusing-xpathexpression-multiple-iterations
        var doc = new XmlDocument();
        doc.LoadXml("<root><a><a1/><a2/></a><b/></root>");

        var nav = doc.CreateNavigator();
        var expr = nav.Compile("*");

        nav.MoveToFirstChild(); // root
        var iter1 = nav.Select(expr);
        iter1.MoveNext(); // root/a
        var iter2 = iter1.Current.Select(expr);
        iter2.MoveNext(); // /root/a/a1
        iter2.MoveNext(); // /root/a/a2

        // used to fail because iter1 and iter2 would conflict
        Assert.IsTrue(iter1.MoveNext()); // root/b
    }
}
