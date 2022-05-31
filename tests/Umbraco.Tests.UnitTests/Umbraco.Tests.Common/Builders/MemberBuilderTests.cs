// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
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
        var testCreateDate = DateTime.Now.AddHours(-1);
        var testUpdateDate = DateTime.Now;
        const int testFailedPasswordAttempts = 22;
        var testLastLockoutDate = DateTime.Now.AddHours(-2);
        var testLastLoginDate = DateTime.Now.AddHours(-3);
        var testLastPasswordChangeDate = DateTime.Now.AddHours(-4);
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
        var testAdditionalData1 = new KeyValuePair<string, object>("test1", 123);
        var testAdditionalData2 = new KeyValuePair<string, object>("test2", "hello");
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
            .AddAdditionalData()
            .WithKeyValue(testAdditionalData1.Key, testAdditionalData1.Value)
            .WithKeyValue(testAdditionalData2.Key, testAdditionalData2.Value)
            .Done()
            .WithPropertyIdsIncrementingFrom(200)
            .AddPropertyData()
            .WithKeyValue(testPropertyData1.Key, testPropertyData1.Value)
            .WithKeyValue(testPropertyData2.Key, testPropertyData2.Value)
            .WithKeyValue(testPropertyData3.Key, testPropertyData3.Value)
            .Done()
            .Build();

        // Assert
        Assert.AreEqual(testMemberTypeId, member.ContentTypeId);
        Assert.AreEqual(testMemberTypeAlias, member.ContentType.Alias);
        Assert.AreEqual(testMemberTypeName, member.ContentType.Name);
        Assert.AreEqual(testId, member.Id);
        Assert.AreEqual(testKey, member.Key);
        Assert.AreEqual(testName, member.Name);
        Assert.AreEqual(testCreateDate, member.CreateDate);
        Assert.AreEqual(testUpdateDate, member.UpdateDate);
        Assert.AreEqual(testCreatorId, member.CreatorId);
        Assert.AreEqual(testFailedPasswordAttempts, member.FailedPasswordAttempts);
        Assert.AreEqual(testIsApproved, member.IsApproved);
        Assert.AreEqual(testIsLockedOut, member.IsLockedOut);
        Assert.AreEqual(testLastLockoutDate, member.LastLockoutDate);
        Assert.AreEqual(testLastLoginDate, member.LastLoginDate);
        Assert.AreEqual(testLastPasswordChangeDate, member.LastPasswordChangeDate);
        Assert.AreEqual(testGroups, member.Groups.ToArray());
        Assert.AreEqual(4, member.Properties.Count); // 1 from membership properties group, 3 custom
        Assert.AreEqual(testPropertyData1.Value, member.GetValue<string>(testPropertyData1.Key));
        Assert.AreEqual(testPropertyData2.Value, member.GetValue<string>(testPropertyData2.Key));
        Assert.AreEqual(testPropertyData3.Value, member.GetValue<string>(testPropertyData3.Key));

        var propertyIds = member.Properties.Select(x => x.Id).OrderBy(x => x).ToArray();
        Assert.AreEqual(testPropertyIdsIncrementingFrom + 1, propertyIds.Min());
        Assert.AreEqual(testPropertyIdsIncrementingFrom + 4, propertyIds.Max());

        Assert.AreEqual(2, member.AdditionalData.Count);
        Assert.AreEqual(testAdditionalData1.Value, member.AdditionalData[testAdditionalData1.Key]);
        Assert.AreEqual(testAdditionalData2.Value, member.AdditionalData[testAdditionalData2.Key]);
    }
}
