// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class MediaTypeBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int testId = 99;
        var testKey = Guid.NewGuid();
        const string testAlias = "mediaType";
        const string testName = "Media Type";
        const string testPropertyGroupName = "Additional Content";
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

        var builder = new MediaTypeBuilder();

        // Act
        var mediaType = builder
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
            .WithMediaPropertyGroup()
            .AddPropertyGroup()
            .WithId(1)
            .WithName(testPropertyGroupName)
            .WithSortOrder(1)
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithAlias(testPropertyType1.Alias)
            .WithName(testPropertyType1.Name)
            .WithSortOrder(testPropertyType1.SortOrder)
            .WithDataTypeId(testPropertyType1.DataTypeId)
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
            .Build();

        // Assert
        Assert.That(mediaType.Id, Is.EqualTo(testId));
        Assert.That(mediaType.Alias, Is.EqualTo(testAlias));
        Assert.That(mediaType.Name, Is.EqualTo(testName));
        Assert.That(mediaType.Key, Is.EqualTo(testKey));
        Assert.That(mediaType.CreateDate, Is.EqualTo(testCreateDate));
        Assert.That(mediaType.UpdateDate, Is.EqualTo(testUpdateDate));
        Assert.That(mediaType.CreatorId, Is.EqualTo(testCreatorId));
        Assert.That(mediaType.ParentId, Is.EqualTo(testParentId));
        Assert.That(mediaType.Level, Is.EqualTo(testLevel));
        Assert.That(mediaType.Path, Is.EqualTo(testPath));
        Assert.That(mediaType.SortOrder, Is.EqualTo(testSortOrder));
        Assert.That(mediaType.Description, Is.EqualTo(testDescription));
        Assert.That(mediaType.Icon, Is.EqualTo(testIcon));
        Assert.That(mediaType.Thumbnail, Is.EqualTo(testThumbnail));
        Assert.That(mediaType.Trashed, Is.EqualTo(testTrashed));
        Assert.That(mediaType.ListView, Is.Null);
        Assert.That(mediaType.PropertyTypes.Count(), Is.EqualTo(7)); // 5 from media properties group, 2 custom

        var propertyTypeIds = mediaType.PropertyTypes.Select(x => x.Id).OrderBy(x => x).ToArray();
        Assert.That(propertyTypeIds.Min(), Is.EqualTo(testPropertyTypeIdsIncrementingFrom + 1));
        Assert.That(propertyTypeIds.Max(), Is.EqualTo(testPropertyTypeIdsIncrementingFrom + 7));
    }
}
