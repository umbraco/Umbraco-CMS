// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using Umbraco.Cms.Core.Xml;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Xml;

[TestFixture]
public class XmlHelperTests
{
    [SetUp]
    public void SetUp() => _builder = new XmlDocumentBuilder();

    private XmlDocumentBuilder _builder;

    [Ignore("This is a benchmark test so is ignored by default")]
    [Test]
    public void Sort_Nodes_Benchmark_Legacy()
    {
        var xml = _builder.Build();
        var original = xml.GetElementById(1173.ToString());
        Assert.IsNotNull(original);

        long totalTime = 0;
        var watch = new Stopwatch();
        var iterations = 10000;

        for (var i = 0; i < iterations; i++)
        {
            // don't measure the time for clone!
            var parentNode = original.Clone();
            watch.Start();
            LegacySortNodes(ref parentNode);
            watch.Stop();
            totalTime += watch.ElapsedMilliseconds;
            watch.Reset();

            // Do assertions just to make sure it is working properly.
            var currSort = 0;
            foreach (var child in parentNode.SelectNodes("./* [@id]").Cast<XmlNode>())
            {
                Assert.AreEqual(currSort, int.Parse(child.Attributes["sortOrder"].Value));
                currSort++;
            }

            // Ensure the parent node's properties still exist first.
            Assert.AreEqual("content", parentNode.ChildNodes[0].Name);
            Assert.AreEqual("umbracoUrlAlias", parentNode.ChildNodes[1].Name);

            // Then the child nodes should come straight after.
            Assert.IsTrue(parentNode.ChildNodes[2].Attributes["id"] != null);
        }

        Debug.WriteLine("Total time for " + iterations + " iterations is " + totalTime);
    }

    [Ignore("This is a benchmark test so is ignored by default")]
    [Test]
    public void Sort_Nodes_Benchmark_New()
    {
        var xml = _builder.Build();
        var original = xml.GetElementById(1173.ToString());
        Assert.IsNotNull(original);

        long totalTime = 0;
        var watch = new Stopwatch();
        var iterations = 10000;

        for (var i = 0; i < iterations; i++)
        {
            // don't measure the time for clone!
            var parentNode = (XmlElement)original.Clone();
            watch.Start();
            XmlHelper.SortNodes(
                parentNode,
                "./* [@id]",
                x => x.AttributeValue<int>("sortOrder"));
            watch.Stop();
            totalTime += watch.ElapsedMilliseconds;
            watch.Reset();

            // do assertions just to make sure it is working properly.
            var currSort = 0;
            foreach (var child in parentNode.SelectNodes("./* [@id]").Cast<XmlNode>())
            {
                Assert.AreEqual(currSort, int.Parse(child.Attributes["sortOrder"].Value));
                currSort++;
            }

            // ensure the parent node's properties still exist first
            Assert.AreEqual("content", parentNode.ChildNodes[0].Name);
            Assert.AreEqual("umbracoUrlAlias", parentNode.ChildNodes[1].Name);

            // then the child nodes should come straight after
            Assert.IsTrue(parentNode.ChildNodes[2].Attributes["id"] != null);
        }

        Debug.WriteLine("Total time for " + iterations + " iterations is " + totalTime);
    }

    [Test]
    public void Sort_Nodes()
    {
        var xml = _builder.Build();
        var original = xml.GetElementById(1173.ToString());
        Assert.IsNotNull(original);

        var parentNode = (XmlElement)original.Clone();

        XmlHelper.SortNodes(
            parentNode,
            "./* [@id]",
            x => x.AttributeValue<int>("sortOrder"));

        // do assertions just to make sure it is working properly.
        var currSort = 0;
        foreach (var child in parentNode.SelectNodes("./* [@id]").Cast<XmlNode>())
        {
            Assert.AreEqual(currSort, int.Parse(child.Attributes["sortOrder"].Value));
            currSort++;
        }

        // ensure the parent node's properties still exist first
        Assert.AreEqual("content", parentNode.ChildNodes[0].Name);
        Assert.AreEqual("umbracoUrlAlias", parentNode.ChildNodes[1].Name);

        // then the child nodes should come straight after
        Assert.IsTrue(parentNode.ChildNodes[2].Attributes["id"] != null);
    }

    /// <summary>
    ///     This was the logic to sort before and now lives here just to show the benchmarks tests above.
    /// </summary>
    private static void LegacySortNodes(ref XmlNode parentNode)
    {
        var n = parentNode.CloneNode(true);

        // remove all children from original node
        var xpath = "./* [@id]";
        foreach (XmlNode child in parentNode.SelectNodes(xpath))
        {
            parentNode.RemoveChild(child);
        }

        var nav = n.CreateNavigator();
        var expr = nav.Compile(xpath);
        expr.AddSort("@sortOrder", XmlSortOrder.Ascending, XmlCaseOrder.None, string.Empty, XmlDataType.Number);
        var iterator = nav.Select(expr);
        while (iterator.MoveNext())
        {
            parentNode.AppendChild(
                ((IHasXmlNode)iterator.Current).GetNode());
        }
    }
}
