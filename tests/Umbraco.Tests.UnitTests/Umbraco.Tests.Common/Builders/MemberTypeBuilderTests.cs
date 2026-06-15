// Copyright (c) Umbraco.
// See LICENSE for more details.

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
        var testCreateDate = DateTime.UtcNow.AddHours(-1);
        var testUpdateDate = DateTime.UtcNow;
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
        Assert.That(memberType.Id, Is.EqualTo(testId));
        Assert.That(memberType.Alias, Is.EqualTo(testAlias));
        Assert.That(memberType.Name, Is.EqualTo(testName));
        Assert.That(memberType.Key, Is.EqualTo(testKey));
        Assert.That(memberType.CreateDate, Is.EqualTo(testCreateDate));
        Assert.That(memberType.UpdateDate, Is.EqualTo(testUpdateDate));
        Assert.That(memberType.CreatorId, Is.EqualTo(testCreatorId));
        Assert.That(memberType.ParentId, Is.EqualTo(testParentId));
        Assert.That(memberType.Level, Is.EqualTo(testLevel));
        Assert.That(memberType.Path, Is.EqualTo(testPath));
        Assert.That(memberType.SortOrder, Is.EqualTo(testSortOrder));
        Assert.That(memberType.Description, Is.EqualTo(testDescription));
        Assert.That(memberType.Icon, Is.EqualTo(testIcon));
        Assert.That(memberType.Thumbnail, Is.EqualTo(testThumbnail));
        Assert.That(memberType.Trashed, Is.EqualTo(testTrashed));
        Assert.That(memberType.ListView, Is.Null);
        Assert.That(memberType.PropertyTypes.Count(), Is.EqualTo(3)); // 1 from membership properties group, 2 custom

        var propertyTypeIds = memberType.PropertyTypes.Select(x => x.Id).OrderBy(x => x).ToArray();
        Assert.That(propertyTypeIds.Min(), Is.EqualTo(testPropertyTypeIdsIncrementingFrom + 1));
        Assert.That(propertyTypeIds.Max(), Is.EqualTo(testPropertyTypeIdsIncrementingFrom + 3));

        Assert.That(memberType.MemberCanEditProperty(testPropertyType1.Alias), Is.True);
        Assert.That(memberType.MemberCanViewProperty(testPropertyType1.Alias), Is.False);
        Assert.That(memberType.MemberCanViewProperty(testPropertyType2.Alias), Is.True);
        Assert.That(memberType.MemberCanEditProperty(testPropertyType2.Alias), Is.False);
    }
}
