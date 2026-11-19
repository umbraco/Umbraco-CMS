// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class PublishedContentExtensionsTests
{
    [TestCase("-1,2", 2, "-1,2", 2, false, true, TestName = "IsDescendant: the same content item")]
    [TestCase("-1,2,3", 3, "-1,2", 2, true, true, TestName = "IsDescendant: a child")]
    [TestCase("-1,2,3,4", 4, "-1,2", 2, true, true, TestName = "IsDescendant: a grandchild")]
    [TestCase("-1,3", 2, "-1,2", 2, false, false, TestName = "IsDescendant: an unrelated sibling")]
    [TestCase("-1", 1, "-1,2", 2, false, false, TestName = "IsDescendant: the tree root")]
    [TestCase("-1,2", 2, "-1,2,3", 3, false, false, TestName = "IsDescendant: an ancestor")]
    [TestCase("-1,21", 2, "-1,2", 2, false, false, TestName = "IsDescendant: an ID beginning with the other's ID")]
    [TestCase("-1,211,3", 3, "-1,21", 2, false, false, TestName = "IsDescendant: below an ID beginning with the other's ID")]
    [TestCase("-1,2,3", 2, "-1,2", 2, false, false, TestName = "IsDescendant: a lower path but not a lower level")]
    public void Can_Detect_Whether_Content_Is_A_Descendant(
        string path,
        int level,
        string otherPath,
        int otherLevel,
        bool expectedIsDescendant,
        bool expectedIsDescendantOrSelf)
    {
        IPublishedContent content = Content(path, level);
        IPublishedContent other = Content(otherPath, otherLevel);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(expectedIsDescendant, content.IsDescendant(other));
            Assert.AreEqual(expectedIsDescendantOrSelf, content.IsDescendantOrSelf(other));
        });
    }

    [TestCase("-1,2", 2, "-1,2", 2, false, true, TestName = "IsAncestor: the same content item")]
    [TestCase("-1,2", 2, "-1,2,3", 3, true, true, TestName = "IsAncestor: a child")]
    [TestCase("-1,2", 2, "-1,2,3,4", 4, true, true, TestName = "IsAncestor: a grandchild")]
    [TestCase("-1,2", 2, "-1,3", 2, false, false, TestName = "IsAncestor: an unrelated sibling")]
    [TestCase("-1,2", 2, "-1", 1, false, false, TestName = "IsAncestor: the tree root")]
    [TestCase("-1,2,3", 3, "-1,2", 2, false, false, TestName = "IsAncestor: a descendant")]
    [TestCase("-1,2", 2, "-1,21", 2, false, false, TestName = "IsAncestor: an ID beginning with this one's ID")]
    [TestCase("-1,21", 2, "-1,211,3", 3, false, false, TestName = "IsAncestor: below an ID beginning with this one's ID")]
    [TestCase("-1,2", 3, "-1,2,3", 3, false, false, TestName = "IsAncestor: a higher path but not a higher level")]
    public void Can_Detect_Whether_Content_Is_An_Ancestor(
        string path,
        int level,
        string otherPath,
        int otherLevel,
        bool expectedIsAncestor,
        bool expectedIsAncestorOrSelf)
    {
        IPublishedContent content = Content(path, level);
        IPublishedContent other = Content(otherPath, otherLevel);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(expectedIsAncestor, content.IsAncestor(other));
            Assert.AreEqual(expectedIsAncestorOrSelf, content.IsAncestorOrSelf(other));
        });
    }

    private static IPublishedContent Content(string path, int level)
        => Mock.Of<IPublishedContent>(x => x.Path == path && x.Level == level);
}
