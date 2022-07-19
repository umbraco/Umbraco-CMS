// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class MemberTypeBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int testId = 99;
        var testKey = Guid.NewGuid();
        const string testAlias = "memberType";
        const string testName = "Member Type";
        const string testPropertyGroupName = "Content";
        const int testParentId = 98;
        const int testCreatorId = 22;
        var testCreateDate = DateTime.Now.AddHours(-1);
        var testUpdateDate = DateTime.Now;
        const int testLevel = 3;
        const string testPath = "-1, 4, 10";
        const int testSortOrder = 5;
        const string testDescription = "The description";
        const string testIcon = "icon";
        const string testThumbnail = "thumnail";
        const bool testTrashed = true;
        const int testPropertyTypeIdsIncrementingFrom = 200;
        var testPropertyType1 =
            new PropertyTypeDetail { Alias = "title", Name = "Title", SortOrder = 1, DataTypeId = -88 };
        var testPropertyType2 =
            new PropertyTypeDetail { Alias = "bodyText", Name = "Body Text", SortOrder = 2, DataTypeId = -87 };
        var testPropertyData1 = new KeyValuePair<string, object>("title", "Name member");

        var builder = new MemberTypeBuilder();

        // Act
        var memberType = builder
            .WithId(testId)
            .WithKey(testKey)
            .WithAlias(testAlias)
            .WithName(testName)
            .WithCreatorId(testCreatorId)
            .WithCreateDate(testCreateDate)
            .WithUpdateDate(testUpdateDate)
            .WithParentId(testParentId)
            .WithLevel(testLevel)
            .WithPath(testPath)
            .WithSortOrder(testSortOrder)
            .WithDescription(testDescription)
            .WithIcon(testIcon)
            .WithThumbnail(testThumbnail)
            .WithTrashed(testTrashed)
            .WithPropertyTypeIdsIncrementingFrom(200)
            .WithMembershipPropertyGroup()
            .AddPropertyGroup()
            .WithId(1)
            .WithName(testPropertyGroupName)
            .WithSortOrder(1)
            .AddPropertyType()
            .WithAlias(testPropertyType1.Alias)
            .WithName(testPropertyType1.Name)
            .WithSortOrder(testPropertyType1.SortOrder)
            .Done()
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithAlias(testPropertyType2.Alias)
            .WithName(testPropertyType2.Name)
            .WithSortOrder(testPropertyType2.SortOrder)
            .WithDataTypeId(testPropertyType2.DataTypeId)
            .Done()
            .Done()
            .WithMemberCanEditProperty(testPropertyType1.Alias, true)
            .WithMemberCanViewProperty(testPropertyType2.Alias, true)
            .Build();

        // Assert
        Assert.AreEqual(testId, memberType.Id);
        Assert.AreEqual(testAlias, memberType.Alias);
        Assert.AreEqual(testName, memberType.Name);
        Assert.AreEqual(testKey, memberType.Key);
        Assert.AreEqual(testCreateDate, memberType.CreateDate);
        Assert.AreEqual(testUpdateDate, memberType.UpdateDate);
        Assert.AreEqual(testCreatorId, memberType.CreatorId);
        Assert.AreEqual(testParentId, memberType.ParentId);
        Assert.AreEqual(testLevel, memberType.Level);
        Assert.AreEqual(testPath, memberType.Path);
        Assert.AreEqual(testSortOrder, memberType.SortOrder);
        Assert.AreEqual(testDescription, memberType.Description);
        Assert.AreEqual(testIcon, memberType.Icon);
        Assert.AreEqual(testThumbnail, memberType.Thumbnail);
        Assert.AreEqual(testTrashed, memberType.Trashed);
        Assert.IsFalse(memberType.IsContainer);
        Assert.AreEqual(3, memberType.PropertyTypes.Count()); // 1 from membership properties group, 2 custom

        var propertyTypeIds = memberType.PropertyTypes.Select(x => x.Id).OrderBy(x => x).ToArray();
        Assert.AreEqual(testPropertyTypeIdsIncrementingFrom + 1, propertyTypeIds.Min());
        Assert.AreEqual(testPropertyTypeIdsIncrementingFrom + 3, propertyTypeIds.Max());

        Assert.IsTrue(memberType.MemberCanEditProperty(testPropertyType1.Alias));
        Assert.IsFalse(memberType.MemberCanViewProperty(testPropertyType1.Alias));
        Assert.IsTrue(memberType.MemberCanViewProperty(testPropertyType2.Alias));
        Assert.IsFalse(memberType.MemberCanEditProperty(testPropertyType2.Alias));
    }
}
