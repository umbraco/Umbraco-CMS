// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering methods in the TagService class.
///     Involves multiple layers as well as configuration.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class TagServiceTests : UmbracoIntegrationTest
{
    [SetUp]
    public void CreateTestData()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template); // else, FK violation on contentType!

        _contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
                defaultTemplateId: template.Id);
        _contentType.PropertyGroups.First().PropertyTypes.Add(
            new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "tags")
            {
                DataTypeId = Constants.DataTypes.Tags
            });
        ContentTypeService.Save(_contentType);
    }

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IFileService FileService => GetRequiredService<IFileService>();

    private ITagService TagService => GetRequiredService<ITagService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IJsonSerializer Serializer => GetRequiredService<IJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IContentType _contentType;

    [Test]
    public void TagApiConsistencyTest()
    {
        IContent content1 = ContentBuilder.CreateSimpleContent(_contentType, "Tagged content 1");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "cow", "pig", "goat" });
        ContentService.SaveAndPublish(content1);

        // change
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "elephant" }, true);
        content1.RemoveTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "cow" });
        ContentService.SaveAndPublish(content1);

        // more changes
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "mouse" }, true);
        ContentService.SaveAndPublish(content1);
        content1.RemoveTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "mouse" });
        ContentService.SaveAndPublish(content1);

        // get it back
        content1 = ContentService.GetById(content1.Id);
        var tagsValue = content1.GetValue("tags").ToString();
        var tagsValues = JsonConvert.DeserializeObject<string[]>(tagsValue);
        Assert.AreEqual(3, tagsValues.Length);
        Assert.Contains("pig", tagsValues);
        Assert.Contains("goat", tagsValues);
        Assert.Contains("elephant", tagsValues);

        var tags = TagService.GetTagsForProperty(content1.Id, "tags").ToArray();
        Assert.IsTrue(tags.All(x => x.Group == "default"));
        tagsValues = tags.Select(x => x.Text).ToArray();

        Assert.AreEqual(3, tagsValues.Length);
        Assert.Contains("pig", tagsValues);
        Assert.Contains("goat", tagsValues);
        Assert.Contains("elephant", tagsValues);
    }

    [Test]
    public void TagList_Contains_NodeCount()
    {
        var content1 = ContentBuilder.CreateSimpleContent(_contentType, "Tagged content 1");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "cow", "pig", "goat" });
        ContentService.SaveAndPublish(content1);

        var content2 = ContentBuilder.CreateSimpleContent(_contentType, "Tagged content 2");
        content2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "cow", "pig" });
        ContentService.SaveAndPublish(content2);

        var content3 = ContentBuilder.CreateSimpleContent(_contentType, "Tagged content 3");
        content3.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "cow" });
        ContentService.SaveAndPublish(content3);

        // Act
        var tags = TagService.GetAllContentTags()
            .OrderByDescending(x => x.NodeCount)
            .ToList();

        // Assert
        Assert.AreEqual(3, tags.Count);
        Assert.AreEqual("cow", tags[0].Text);
        Assert.AreEqual(3, tags[0].NodeCount);
        Assert.AreEqual("pig", tags[1].Text);
        Assert.AreEqual(2, tags[1].NodeCount);
        Assert.AreEqual("goat", tags[2].Text);
        Assert.AreEqual(1, tags[2].NodeCount);
    }
}
