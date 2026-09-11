// Copyright (c) Umbraco.
// See LICENSE for more details.

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
        var testTemplate1 = new TemplateDetail { Id = 200, Alias = "template1", Name = "Template 1" };
        var testTemplate2 = new TemplateDetail { Id = 201, Alias = "template2", Name = "Template 2" };
        var testAllowedContentType1 = new AllowedContentTypeDetail { Key = new Guid("72EC4F7B-ACF0-43AA-AD92-0EC878223485"), Alias = "subType1", SortOrder = 1 };
        var testAllowedContentType2 = new AllowedContentTypeDetail { Key = new Guid("68FE62F0-95A9-471E-839F-F5A6B9CCA7A9"), Alias = "subType2", SortOrder = 2 };

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
            .WithKey(testAllowedContentType1.Key)
            .WithAlias(testAllowedContentType1.Alias)
            .WithSortOrder(testAllowedContentType1.SortOrder)
            .Done()
            .AddAllowedContentType()
            .WithKey(testAllowedContentType2.Key)
            .WithAlias(testAllowedContentType2.Alias)
            .WithSortOrder(testAllowedContentType2.SortOrder)
            .Done()
            .Build();

        // Assert
        Assert.That(contentType.Id, Is.EqualTo(testId));
        Assert.That(contentType.Alias, Is.EqualTo(testAlias));
        Assert.That(contentType.Name, Is.EqualTo(testName));
        Assert.That(contentType.Key, Is.EqualTo(testKey));
        Assert.That(contentType.CreateDate, Is.EqualTo(testCreateDate));
        Assert.That(contentType.UpdateDate, Is.EqualTo(testUpdateDate));
        Assert.That(contentType.CreatorId, Is.EqualTo(testCreatorId));
        Assert.That(contentType.ParentId, Is.EqualTo(testParentId));
        Assert.That(contentType.Level, Is.EqualTo(testLevel));
        Assert.That(contentType.Path, Is.EqualTo(testPath));
        Assert.That(contentType.SortOrder, Is.EqualTo(testSortOrder));
        Assert.That(contentType.Description, Is.EqualTo(testDescription));
        Assert.That(contentType.Icon, Is.EqualTo(testIcon));
        Assert.That(contentType.Thumbnail, Is.EqualTo(testThumbnail));
        Assert.That(contentType.Trashed, Is.EqualTo(testTrashed));
        Assert.That(contentType.ListView, Is.Null);
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(2));

        var propertyTypeIds = contentType.PropertyTypes.Select(x => x.Id).OrderBy(x => x).ToArray();
        Assert.That(propertyTypeIds.Min(), Is.EqualTo(testPropertyTypeIdsIncrementingFrom + 1));
        Assert.That(propertyTypeIds.Max(), Is.EqualTo(testPropertyTypeIdsIncrementingFrom + 2));

        var allowedTemplates = contentType.AllowedTemplates.ToList();
        Assert.That(allowedTemplates, Has.Count.EqualTo(2));
        Assert.That(allowedTemplates[0].Id, Is.EqualTo(testTemplate1.Id));
        Assert.That(allowedTemplates[0].Alias, Is.EqualTo(testTemplate1.Alias));
        Assert.That(allowedTemplates[0].Name, Is.EqualTo(testTemplate1.Name));
        Assert.That(contentType.DefaultTemplate.Id, Is.EqualTo(testTemplate1.Id));

        var allowedContentTypes = contentType.AllowedContentTypes.ToList();
        Assert.That(allowedContentTypes, Has.Count.EqualTo(2));
        Assert.That(allowedContentTypes[0].Key, Is.EqualTo(testAllowedContentType1.Key));
        Assert.That(allowedContentTypes[0].Alias, Is.EqualTo(testAllowedContentType1.Alias));
        Assert.That(allowedContentTypes[0].SortOrder, Is.EqualTo(testAllowedContentType1.SortOrder));
    }
}
