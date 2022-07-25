// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class ContentTypeBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int testId = 99;
        var testKey = Guid.NewGuid();
        const string testAlias = "mediaType";
        const string testName = "Content Type";
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
        var testTemplate1 = new TemplateDetail { Id = 200, Alias = "template1", Name = "Template 1" };
        var testTemplate2 = new TemplateDetail { Id = 201, Alias = "template2", Name = "Template 2" };
        var testAllowedContentType1 = new AllowedContentTypeDetail { Id = 300, Alias = "subType1", SortOrder = 1 };
        var testAllowedContentType2 = new AllowedContentTypeDetail { Id = 301, Alias = "subType2", SortOrder = 2 };

        var builder = new ContentTypeBuilder();

        // Act
        var contentType = builder
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
            .AddPropertyGroup()
            .WithName(testPropertyGroupName)
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
            .Done()
            .AddAllowedTemplate()
            .WithId(testTemplate1.Id)
            .WithAlias(testTemplate1.Alias)
            .WithName(testTemplate1.Name)
            .Done()
            .AddAllowedTemplate()
            .WithId(testTemplate2.Id)
            .WithAlias(testTemplate2.Alias)
            .WithName(testTemplate2.Name)
            .Done()
            .WithDefaultTemplateId(testTemplate1.Id)
            .AddAllowedContentType()
            .WithId(testAllowedContentType1.Id)
            .WithAlias(testAllowedContentType1.Alias)
            .WithSortOrder(testAllowedContentType1.SortOrder)
            .Done()
            .AddAllowedContentType()
            .WithId(testAllowedContentType2.Id)
            .WithAlias(testAllowedContentType2.Alias)
            .WithSortOrder(testAllowedContentType2.SortOrder)
            .Done()
            .Build();

        // Assert
        Assert.AreEqual(testId, contentType.Id);
        Assert.AreEqual(testAlias, contentType.Alias);
        Assert.AreEqual(testName, contentType.Name);
        Assert.AreEqual(testKey, contentType.Key);
        Assert.AreEqual(testCreateDate, contentType.CreateDate);
        Assert.AreEqual(testUpdateDate, contentType.UpdateDate);
        Assert.AreEqual(testCreatorId, contentType.CreatorId);
        Assert.AreEqual(testParentId, contentType.ParentId);
        Assert.AreEqual(testLevel, contentType.Level);
        Assert.AreEqual(testPath, contentType.Path);
        Assert.AreEqual(testSortOrder, contentType.SortOrder);
        Assert.AreEqual(testDescription, contentType.Description);
        Assert.AreEqual(testIcon, contentType.Icon);
        Assert.AreEqual(testThumbnail, contentType.Thumbnail);
        Assert.AreEqual(testTrashed, contentType.Trashed);
        Assert.IsFalse(contentType.IsContainer);
        Assert.AreEqual(2, contentType.PropertyTypes.Count());

        var propertyTypeIds = contentType.PropertyTypes.Select(x => x.Id).OrderBy(x => x).ToArray();
        Assert.AreEqual(testPropertyTypeIdsIncrementingFrom + 1, propertyTypeIds.Min());
        Assert.AreEqual(testPropertyTypeIdsIncrementingFrom + 2, propertyTypeIds.Max());

        var allowedTemplates = contentType.AllowedTemplates.ToList();
        Assert.AreEqual(2, allowedTemplates.Count);
        Assert.AreEqual(testTemplate1.Id, allowedTemplates[0].Id);
        Assert.AreEqual(testTemplate1.Alias, allowedTemplates[0].Alias);
        Assert.AreEqual(testTemplate1.Name, allowedTemplates[0].Name);
        Assert.AreEqual(testTemplate1.Id, contentType.DefaultTemplate.Id);

        var allowedContentTypes = contentType.AllowedContentTypes.ToList();
        Assert.AreEqual(2, allowedContentTypes.Count);
        Assert.AreEqual(testAllowedContentType1.Id, allowedContentTypes[0].Id.Value);
        Assert.AreEqual(testAllowedContentType1.Alias, allowedContentTypes[0].Alias);
        Assert.AreEqual(testAllowedContentType1.SortOrder, allowedContentTypes[0].SortOrder);
    }
}
