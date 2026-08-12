using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
internal sealed class EntityTypeContainerServiceTests
{
    // The helper is static, so any set of type arguments reaches the same implementation.
    private static bool IsSelfOrDescendant(string path, string containerPath)
        => EntityTypeContainerService<IContentType, IDocumentTypeContainerRepository>.IsSelfOrDescendant(path, containerPath);

    [TestCase("-1,2", "-1,2", true, TestName = "The container itself")]
    [TestCase("-1,2,3", "-1,2", true, TestName = "A child of the container")]
    [TestCase("-1,2,3,4", "-1,2", true, TestName = "A grandchild of the container")]
    [TestCase("-1,3", "-1,2", false, TestName = "An unrelated sibling")]
    [TestCase("-1", "-1,2", false, TestName = "The tree root")]
    [TestCase("-1,21", "-1,2", false, TestName = "An ID that merely starts with the container's ID")]
    [TestCase("-1,2113", "-1,211", false, TestName = "A deeper ID that merely starts with the container's ID")]
    [TestCase("-1,211,3", "-1,21", false, TestName = "A child of an ID that merely starts with the container's ID")]
    public void Can_Detect_Whether_A_Path_Is_The_Container_Or_Below_It(string path, string containerPath, bool expected)
        => Assert.AreEqual(expected, IsSelfOrDescendant(path, containerPath));
}
