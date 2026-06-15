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
public class MemberTests
{
    [SetUp]
    public void SetUp() => _builder = new MemberBuilder();

    private MemberBuilder _builder;

    [Test]
    public void Can_Deep_Clone()
    {
        // Arrange
        var member = BuildMember();

        // Act
        var clone = (Member)member.DeepClone();

        // Assert
        Assert.That(member, Is.Not.SameAs(clone));
        Assert.That(member, Is.EqualTo(clone));
        Assert.That(member.Id, Is.EqualTo(clone.Id));
        Assert.That(member.VersionId, Is.EqualTo(clone.VersionId));
        Assert.That(member.ContentType, Is.EqualTo(clone.ContentType));
        Assert.That(member.ContentTypeId, Is.EqualTo(clone.ContentTypeId));
        Assert.That(member.CreateDate, Is.EqualTo(clone.CreateDate));
        Assert.That(member.CreatorId, Is.EqualTo(clone.CreatorId));
        Assert.That(member.Comments, Is.EqualTo(clone.Comments));
        Assert.That(member.Key, Is.EqualTo(clone.Key));
        Assert.That(member.FailedPasswordAttempts, Is.EqualTo(clone.FailedPasswordAttempts));
        Assert.That(member.Level, Is.EqualTo(clone.Level));
        Assert.That(member.Path, Is.EqualTo(clone.Path));
        Assert.That(member.Groups, Is.EqualTo(clone.Groups));
        Assert.That(member.Groups.Count(), Is.EqualTo(clone.Groups.Count()));
        Assert.That(member.IsApproved, Is.EqualTo(clone.IsApproved));
        Assert.That(member.IsLockedOut, Is.EqualTo(clone.IsLockedOut));
        Assert.That(member.SortOrder, Is.EqualTo(clone.SortOrder));
        Assert.That(member.LastLockoutDate, Is.EqualTo(clone.LastLockoutDate));
        Assert.That(member.LastLoginDate, Is.Not.EqualTo(clone.LastLoginDate));
        Assert.That(member.LastPasswordChangeDate, Is.EqualTo(clone.LastPasswordChangeDate));
        Assert.That(member.Trashed, Is.EqualTo(clone.Trashed));
        Assert.That(member.UpdateDate, Is.EqualTo(clone.UpdateDate));
        Assert.That(member.VersionId, Is.EqualTo(clone.VersionId));
        Assert.That(member.RawPasswordValue, Is.EqualTo(clone.RawPasswordValue));
        Assert.That(member.Properties, Is.Not.SameAs(clone.Properties));
        Assert.That(member.Properties.Count(), Is.EqualTo(clone.Properties.Count()));
        for (var index = 0; index < member.Properties.Count; index++)
        {
            Assert.That(member.Properties[index], Is.Not.SameAs(clone.Properties[index]));
            Assert.That(member.Properties[index], Is.EqualTo(clone.Properties[index]));
        }

        // this can be the same, it is immutable
        Assert.That(member.ContentType, Is.SameAs(clone.ContentType));

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.That(propertyInfo.GetValue(member, null), Is.EqualTo(propertyInfo.GetValue(clone, null)));
        }
    }

    [Test]
    public void Can_Serialize_Without_Error()
    {
        var member = BuildMember();

        var json = JsonSerializer.Serialize(member);
        Debug.Print(json);
    }

    private Member BuildMember() =>
        _builder
            .AddMemberType()
            .WithId(99)
            .WithAlias("memberType")
            .WithName("Member Type")
            .WithMembershipPropertyGroup()
            .AddPropertyGroup()
            .WithName("Content")
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithSortOrder(1)
            .Done()
            .AddPropertyType()
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithAlias("bodyText")
            .WithName("Body text")
            .WithSortOrder(2)
            .WithDataTypeId(-87)
            .Done()
            .AddPropertyType()
            .WithAlias("author")
            .WithName("Author")
            .WithDescription("Name of the author")
            .WithSortOrder(3)
            .Done()
            .Done()
            .Done()
            .WithId(10)
            .WithName("Fred")
            .WithLogin("fred", "raw pass")
            .WithEmail("email@email.com")
            .WithFailedPasswordAttempts(22)
            .WithIsApproved(true)
            .WithIsLockedOut(true)
            .WithTrashed(false)
            .AddMemberGroups()
            .WithValue("Group 1")
            .WithValue("Group 2")
            .Done()
            .WithPropertyIdsIncrementingFrom(200)
            .AddPropertyData()
            .WithKeyValue("title", "Name member")
            .WithKeyValue("bodyText", "This is a subpage")
            .WithKeyValue("author", "John Doe")
            .Done()
            .Build();
}
