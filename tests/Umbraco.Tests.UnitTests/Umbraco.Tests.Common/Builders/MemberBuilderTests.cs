// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class MemberBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int testMemberTypeId = 99;
        const string testMemberTypeAlias = "memberType";
        const string testMemberTypeName = "Member Type";
        const string testMemberTypePropertyGroupName = "Content";
        const int testId = 10;
        const string testName = "Fred";
        const string testUsername = "fred";
        const string testRawPasswordValue = "raw pass";
        const string testEmail = "email@email.com";
        const int testCreatorId = 22;
        const int testLevel = 3;
        const string testPath = "-1, 4, 10";
        const bool testIsApproved = true;
        const bool testIsLockedOut = true;
        const int testSortOrder = 5;
        const bool testTrashed = false;
        var testKey = Guid.NewGuid();
        var testCreateDate = DateTime.UtcNow.AddHours(-1);
        var testUpdateDate = DateTime.UtcNow;
        const int testFailedPasswordAttempts = 22;
        var testLastLockoutDate = DateTime.UtcNow.AddHours(-2);
        var testLastLoginDate = DateTime.UtcNow.AddHours(-3);
        var testLastPasswordChangeDate = DateTime.UtcNow.AddHours(-4);
        var testPropertyType1 =
            new PropertyTypeDetail { Alias = "title", Name = "Title", SortOrder = 1, DataTypeId = -88 };
        var testPropertyType2 =
            new PropertyTypeDetail { Alias = "bodyText", Name = "Body Text", SortOrder = 2, DataTypeId = -87 };
        var testPropertyType3 = new PropertyTypeDetail
        {
            Alias = "author",
            Name = "Author",
            Description = "Writer of the article",
            SortOrder = 1,
            DataTypeId = -88,
        };
        var testGroups = new[] { "group1", "group2" };
        var testPropertyData1 = new KeyValuePair<string, object>("title", "Name member");
        var testPropertyData2 = new KeyValuePair<string, object>("bodyText", "This is a subpage");
        var testPropertyData3 = new KeyValuePair<string, object>("author", "John Doe");
        const int testPropertyIdsIncrementingFrom = 200;

        var builder = new MemberBuilder();

        // Act
        var member = builder
            .AddMemberType()
            .WithId(testMemberTypeId)
            .WithAlias(testMemberTypeAlias)
            .WithName(testMemberTypeName)
            .WithMembershipPropertyGroup()
            .AddPropertyGroup()
            .WithId(1)
            .WithName(testMemberTypePropertyGroupName)
            .WithSortOrder(1)
            .AddPropertyType()
            .WithAlias(testPropertyType1.Alias)
            .WithName(testPropertyType1.Name)
            .WithSortOrder(testPropertyType1.SortOrder)
            .WithDataTypeId(testPropertyType1.DataTypeId)
            .Done()
            .AddPropertyType()
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithAlias(testPropertyType2.Alias)
            .WithName(testPropertyType2.Name)
            .WithSortOrder(testPropertyType2.SortOrder)
            .WithDataTypeId(testPropertyType2.DataTypeId)
            .Done()
            .AddPropertyType()
            .WithAlias(testPropertyType3.Alias)
            .WithName(testPropertyType3.Name)
            .WithDescription(testPropertyType3.Description)
            .WithSortOrder(testPropertyType3.SortOrder)
            .WithDataTypeId(testPropertyType3.DataTypeId)
            .Done()
            .Done()
            .Done()
            .WithId(testId)
            .WithKey(testKey)
            .WithName(testName)
            .WithLogin(testUsername, testRawPasswordValue)
            .WithEmail(testEmail)
            .WithCreatorId(testCreatorId)
            .WithCreateDate(testCreateDate)
            .WithUpdateDate(testUpdateDate)
            .WithLevel(testLevel)
            .WithPath(testPath)
            .WithFailedPasswordAttempts(testFailedPasswordAttempts)
            .WithIsApproved(testIsApproved)
            .WithIsLockedOut(testIsLockedOut, testLastLockoutDate)
            .WithLastLoginDate(testLastLoginDate)
            .WithLastPasswordChangeDate(testLastPasswordChangeDate)
            .WithSortOrder(testSortOrder)
            .WithTrashed(testTrashed)
            .AddMemberGroups()
            .WithValue(testGroups[0])
            .WithValue(testGroups[1])
            .Done()
            .WithPropertyIdsIncrementingFrom(200)
            .AddPropertyData()
            .WithKeyValue(testPropertyData1.Key, testPropertyData1.Value)
            .WithKeyValue(testPropertyData2.Key, testPropertyData2.Value)
            .WithKeyValue(testPropertyData3.Key, testPropertyData3.Value)
            .Done()
            .Build();

        // Assert
        Assert.That(member.ContentTypeId, Is.EqualTo(testMemberTypeId));
        Assert.That(member.ContentType.Alias, Is.EqualTo(testMemberTypeAlias));
        Assert.That(member.ContentType.Name, Is.EqualTo(testMemberTypeName));
        Assert.That(member.Id, Is.EqualTo(testId));
        Assert.That(member.Key, Is.EqualTo(testKey));
        Assert.That(member.Name, Is.EqualTo(testName));
        Assert.That(member.CreateDate, Is.EqualTo(testCreateDate));
        Assert.That(member.UpdateDate, Is.EqualTo(testUpdateDate));
        Assert.That(member.CreatorId, Is.EqualTo(testCreatorId));
        Assert.That(member.FailedPasswordAttempts, Is.EqualTo(testFailedPasswordAttempts));
        Assert.That(member.IsApproved, Is.EqualTo(testIsApproved));
        Assert.That(member.IsLockedOut, Is.EqualTo(testIsLockedOut));
        Assert.That(member.LastLockoutDate, Is.EqualTo(testLastLockoutDate));
        Assert.That(member.LastLoginDate, Is.EqualTo(testLastLoginDate));
        Assert.That(member.LastPasswordChangeDate, Is.EqualTo(testLastPasswordChangeDate));
        Assert.That(member.Groups.ToArray(), Is.EqualTo(testGroups));
        Assert.That(member.Properties, Has.Count.EqualTo(4)); // 1 from membership properties group, 3 custom
        Assert.That(member.GetValue<string>(testPropertyData1.Key), Is.EqualTo(testPropertyData1.Value));
        Assert.That(member.GetValue<string>(testPropertyData2.Key), Is.EqualTo(testPropertyData2.Value));
        Assert.That(member.GetValue<string>(testPropertyData3.Key), Is.EqualTo(testPropertyData3.Value));

        var propertyIds = member.Properties.Select(x => x.Id).OrderBy(x => x).ToArray();
        Assert.That(propertyIds.Min(), Is.EqualTo(testPropertyIdsIncrementingFrom + 1));
        Assert.That(propertyIds.Max(), Is.EqualTo(testPropertyIdsIncrementingFrom + 4));
    }
}
