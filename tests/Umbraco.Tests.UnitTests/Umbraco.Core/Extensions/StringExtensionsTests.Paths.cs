using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

public partial class StringExtensionsTests
{
    [TestCase("-1,2", "-1,2", false, TestName = "IsDescendantOfPath: the node itself")]
    [TestCase("-1,2,3", "-1,2", true, TestName = "IsDescendantOfPath: a child")]
    [TestCase("-1,2,3,4", "-1,2", true, TestName = "IsDescendantOfPath: a grandchild")]
    [TestCase("-1,3", "-1,2", false, TestName = "IsDescendantOfPath: an unrelated sibling")]
    [TestCase("-1", "-1,2", false, TestName = "IsDescendantOfPath: the tree root")]
    [TestCase("-1,21", "-1,2", false, TestName = "IsDescendantOfPath: an ID beginning with the ancestor's ID")]
    [TestCase("-1,2113", "-1,211", false, TestName = "IsDescendantOfPath: a deeper ID beginning with the ancestor's ID")]
    [TestCase("-1,211,3", "-1,21", false, TestName = "IsDescendantOfPath: below an ID beginning with the ancestor's ID")]
    public void Can_Detect_Whether_A_Path_Is_Below_Another(string path, string ancestorPath, bool expected)
        => Assert.AreEqual(expected, path.IsDescendantOfPath(ancestorPath));

    [TestCase("-1,2", "-1,2", true, TestName = "IsDescendantOrSelfOfPath: the node itself")]
    [TestCase("-1,2,3", "-1,2", true, TestName = "IsDescendantOrSelfOfPath: a child")]
    [TestCase("-1,2,3,4", "-1,2", true, TestName = "IsDescendantOrSelfOfPath: a grandchild")]
    [TestCase("-1,3", "-1,2", false, TestName = "IsDescendantOrSelfOfPath: an unrelated sibling")]
    [TestCase("-1", "-1,2", false, TestName = "IsDescendantOrSelfOfPath: the tree root")]
    [TestCase("-1,21", "-1,2", false, TestName = "IsDescendantOrSelfOfPath: an ID beginning with the ancestor's ID")]
    [TestCase("-1,2113", "-1,211", false, TestName = "IsDescendantOrSelfOfPath: a deeper ID beginning with the ancestor's ID")]
    [TestCase("-1,211,3", "-1,21", false, TestName = "IsDescendantOrSelfOfPath: below an ID beginning with the ancestor's ID")]
    public void Can_Detect_Whether_A_Path_Is_Another_Or_Below_It(string path, string ancestorPath, bool expected)
        => Assert.AreEqual(expected, path.IsDescendantOrSelfOfPath(ancestorPath));
}
