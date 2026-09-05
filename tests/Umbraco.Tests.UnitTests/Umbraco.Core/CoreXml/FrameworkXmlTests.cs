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
        Assert.That(node1, Is.Not.Null);

        var doc2 = new XmlDocument();
        doc2.LoadXml("<nodes />");
        var node2 = doc2.ImportNode(node1, true);
        var root2 = doc2.DocumentElement;
        Assert.That(root2, Is.Not.Null);
        root2.AppendChild(node2);

        var node3 = doc2.SelectSingleNode("//item2");

        Assert.That(node2, Is.Not.SameAs(node1)); // has been cloned
        Assert.That(node3, Is.SameAs(node2)); // has been appended

        Assert.That(node2.FirstChild, Is.Not.SameAs(node1.FirstChild)); // deep clone
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

        Assert.That(nav1.MoveToFirstChild(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("root"));
        Assert.That(nav1.MoveToFirstChild(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("items"));
        Assert.That(nav1.MoveToFirstChild(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("item1"));
        Assert.That(nav1.MoveToNext(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("item2"));

        var node1 = doc1.SelectSingleNode("//item2");
        Assert.That(node1, Is.Not.Null);
        var parent1 = node1.ParentNode;
        Assert.That(parent1, Is.Not.Null);
        parent1.RemoveChild(node1);

        // navigator now navigates on an isolated fragment
        // that is rooted on the node that was removed
        Assert.That(nav1.Name, Is.EqualTo("item2"));
        Assert.That(nav1.MoveToPrevious(), Is.False);
        Assert.That(nav1.MoveToNext(), Is.False);
        Assert.That(nav1.MoveToParent(), Is.False);

        Assert.That(nav1.MoveToFirstChild(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("item21"));
        Assert.That(nav1.MoveToParent(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("item2"));

        nav1.MoveToRoot();
        Assert.That(nav1.Name, Is.EqualTo("item2"));
    }

    [Test]
    public void CanRemovePathNodeAndNavigate()
    {
        var doc1 = new XmlDocument();
        doc1.LoadXml(Xml1);
        var nav1 = doc1.CreateNavigator();

        Assert.That(nav1.MoveToFirstChild(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("root"));
        Assert.That(nav1.MoveToFirstChild(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("items"));
        Assert.That(nav1.MoveToFirstChild(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("item1"));
        Assert.That(nav1.MoveToNext(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("item2"));
        Assert.That(nav1.MoveToFirstChild(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("item21"));

        var node1 = doc1.SelectSingleNode("//item2");
        Assert.That(node1, Is.Not.Null);
        var parent1 = node1.ParentNode;
        Assert.That(parent1, Is.Not.Null);
        parent1.RemoveChild(node1);

        // navigator now navigates on an isolated fragment
        // that is rooted on the node that was removed
        Assert.That(nav1.Name, Is.EqualTo("item21"));
        Assert.That(nav1.MoveToParent(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("item2"));
        Assert.That(nav1.MoveToPrevious(), Is.False);
        Assert.That(nav1.MoveToNext(), Is.False);
        Assert.That(nav1.MoveToParent(), Is.False);

        nav1.MoveToRoot();
        Assert.That(nav1.Name, Is.EqualTo("item2"));
    }

    [Test]
    public void CanRemoveOutOfPathNodeAndNavigate()
    {
        var doc1 = new XmlDocument();
        doc1.LoadXml(Xml1);
        var nav1 = doc1.CreateNavigator();

        Assert.That(nav1.MoveToFirstChild(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("root"));
        Assert.That(nav1.MoveToFirstChild(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("items"));
        Assert.That(nav1.MoveToFirstChild(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("item1"));
        Assert.That(nav1.MoveToNext(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("item2"));
        Assert.That(nav1.MoveToNext(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("item3"));
        Assert.That(nav1.MoveToNext(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("item4"));

        var node1 = doc1.SelectSingleNode("//item2");
        Assert.That(node1, Is.Not.Null);
        var parent1 = node1.ParentNode;
        Assert.That(parent1, Is.Not.Null);
        parent1.RemoveChild(node1);

        // navigator sees the change
        Assert.That(nav1.Name, Is.EqualTo("item4"));
        Assert.That(nav1.MoveToPrevious(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("item3"));
        Assert.That(nav1.MoveToPrevious(), Is.True);
        Assert.That(nav1.Name, Is.EqualTo("item1"));
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

        Assert.That(iter1, Has.Count.EqualTo(6));

        var node1 = doc1.SelectSingleNode("//item2");
        Assert.That(node1, Is.Not.Null);
        var parent1 = node1.ParentNode;
        Assert.That(parent1, Is.Not.Null);
        parent1.RemoveChild(node1);

        // iterator partially sees the change
        Assert.That(iter1, Has.Count.EqualTo(6)); // has been cached, not updated
        Assert.That(iter2, Has.Count.EqualTo(5)); // not calculated yet, correct value

        var count = 0;
        while (iter1.MoveNext())
        {
            count++;
        }

        Assert.That(count, Is.EqualTo(5));
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
        Assert.That(iter1.MoveNext(), Is.True); // root/b
    }
}
