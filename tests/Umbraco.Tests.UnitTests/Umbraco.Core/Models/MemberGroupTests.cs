// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class MemberGroupTests
{
    [SetUp]
    public void SetUp() => _builder = new MemberGroupBuilder();

    private MemberGroupBuilder _builder;

    [Test]
    public void Can_Deep_Clone()
    {
        // Arrange
        var group = BuildMemberGroup();

        // Act
        var clone = (MemberGroup)group.DeepClone();

        // Assert
        Assert.That(group, Is.Not.SameAs(clone));
        Assert.That(group, Is.EqualTo(clone));
        Assert.That(group.Id, Is.EqualTo(clone.Id));
        Assert.That(group.CreateDate, Is.EqualTo(clone.CreateDate));
        Assert.That(group.CreatorId, Is.EqualTo(clone.CreatorId));
        Assert.That(group.Key, Is.EqualTo(clone.Key));
        Assert.That(group.UpdateDate, Is.EqualTo(clone.UpdateDate));
        Assert.That(group.Name, Is.EqualTo(clone.Name));

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.That(propertyInfo.GetValue(group, null), Is.EqualTo(propertyInfo.GetValue(clone, null)));
        }
    }

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
