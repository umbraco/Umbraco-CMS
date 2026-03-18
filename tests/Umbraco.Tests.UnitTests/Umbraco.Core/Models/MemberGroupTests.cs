// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

/// <summary>
/// Contains unit tests for the <see cref="MemberGroup"/> model in the Umbraco CMS core models.
/// </summary>
[TestFixture]
public class MemberGroupTests
{
    /// <summary>
    /// Sets up the test environment before each test is run.
    /// </summary>
    [SetUp]
    public void SetUp() => _builder = new MemberGroupBuilder();

    private MemberGroupBuilder _builder;

    /// <summary>
    /// Tests that a MemberGroup can be deep cloned correctly.
    /// </summary>
    [Test]
    public void Can_Deep_Clone()
    {
        // Arrange
        var group = BuildMemberGroup();

        // Act
        var clone = (MemberGroup)group.DeepClone();

        // Assert
        Assert.AreNotSame(clone, group);
        Assert.AreEqual(clone, group);
        Assert.AreEqual(clone.Id, group.Id);
        Assert.AreEqual(clone.CreateDate, group.CreateDate);
        Assert.AreEqual(clone.CreatorId, group.CreatorId);
        Assert.AreEqual(clone.Key, group.Key);
        Assert.AreEqual(clone.UpdateDate, group.UpdateDate);
        Assert.AreEqual(clone.Name, group.Name);

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(group, null));
        }
    }

    /// <summary>
    /// Tests that a MemberGroup object can be serialized to JSON without throwing an error.
    /// </summary>
    [Test]
    public void Can_Serialize_Without_Error()
    {
        var group = BuildMemberGroup();

        var json = JsonSerializer.Serialize(group);
        Debug.Print(json);
    }

    private MemberGroup BuildMemberGroup() =>
        _builder
            .WithId(6)
            .WithKey(Guid.NewGuid())
            .WithName("Test Group")
            .WithCreatorId(4)
            .WithCreateDate(DateTime.UtcNow)
            .WithUpdateDate(DateTime.UtcNow)
            .Build();
}
