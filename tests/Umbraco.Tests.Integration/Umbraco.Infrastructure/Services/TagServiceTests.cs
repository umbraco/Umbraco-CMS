// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering methods in the TagService class.
///     Involves multiple layers as well as configuration.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class TagServiceTests : UmbracoIntegrationTest
{
    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();
    [SetUp]
    public async Task CreateTestData()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey); // else, FK violation on contentType!

        _contentType =
            ContentTypeBuilder.CreateSimpleContentType(
                "umbMandatory",
                "Mandatory Doc Type",
                defaultTemplateId: template.Id);
        _contentType.PropertyGroups.First().PropertyTypes.Add(
            new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "tags")
            {
                DataTypeId = Constants.DataTypes.Tags
            });
        await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);
    }

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IElementService ElementService => GetRequiredService<IElementService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ITagService TagService => GetRequiredService<ITagService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IJsonSerializer Serializer => GetRequiredService<IJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IContentType _contentType;

    [Test]
    public void TagApiConsistencyTest()
    {
        IContent content1 = ContentBuilder.CreateSimpleContent(_contentType, "Tagged content 1");
        content1.AssignTags(
            PropertyEditorCollection,
            DataTypeService,
            IdKeyMap,
            Serializer,
            "tags",
            new[] { "cow", "pig", "goat" });
        ContentService.Save(content1);
        ContentService.Publish(content1, Array.Empty<string>());

        // change
        content1.AssignTags(PropertyEditorCollection, DataTypeService, IdKeyMap, Serializer, "tags", new[] { "elephant" }, true);
        content1.RemoveTags(PropertyEditorCollection, DataTypeService, IdKeyMap, Serializer, "tags", new[] { "cow" });
        ContentService.Save(content1);
        ContentService.Publish(content1, Array.Empty<string>());

        // more changes
        content1.AssignTags(PropertyEditorCollection, DataTypeService, IdKeyMap, Serializer, "tags", new[] { "mouse" }, true);
        ContentService.Save(content1);
        ContentService.Publish(content1, Array.Empty<string>());
        content1.RemoveTags(PropertyEditorCollection, DataTypeService, IdKeyMap, Serializer, "tags", new[] { "mouse" });
        ContentService.Save(content1);
        ContentService.Publish(content1, Array.Empty<string>());

        // get it back
        content1 = ContentService.GetById(content1.Id);
        var tagsValue = content1.GetValue("tags").ToString();
        var tagsValues = JsonSerializer.Deserialize<string[]>(tagsValue);
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
    public async Task GetTaggedElementsByTag_Returns_Tagged_Elements()
    {
        IContentType elementType = await CreateElementTypeWithTags();

        IElement element1 = new Element("Tagged element 1", elementType);
        element1.AssignTags(PropertyEditorCollection, DataTypeService, IdKeyMap, Serializer, "tags", new[] { "cow", "pig", "goat" });
        ElementService.Save(element1);
        ElementService.Publish(element1, Array.Empty<string>());

        IElement element2 = new Element("Tagged element 2", elementType);
        element2.AssignTags(PropertyEditorCollection, DataTypeService, IdKeyMap, Serializer, "tags", new[] { "pig" });
        ElementService.Save(element2);
        ElementService.Publish(element2, Array.Empty<string>());

        var taggedWithCow = TagService.GetTaggedElementsByTag("cow").ToArray();
        Assert.AreEqual(1, taggedWithCow.Length);
        Assert.AreEqual(element1.Id, taggedWithCow[0].EntityId);
        Assert.IsTrue(taggedWithCow[0].TaggedProperties.SelectMany(x => x.Tags).Any(x => x.Text == "cow"));

        var taggedWithPig = TagService.GetTaggedElementsByTag("pig").Select(x => x.EntityId).ToArray();
        Assert.AreEqual(2, taggedWithPig.Length);
        CollectionAssert.AreEquivalent(new[] { element1.Id, element2.Id }, taggedWithPig);
    }

    [Test]
    public async Task GetTaggedElementsByTagGroup_Returns_Tagged_Elements()
    {
        IContentType elementType = await CreateElementTypeWithTags();

        IElement element1 = new Element("Tagged element 1", elementType);
        element1.AssignTags(PropertyEditorCollection, DataTypeService, IdKeyMap, Serializer, "tags", new[] { "cow", "pig" });
        ElementService.Save(element1);
        ElementService.Publish(element1, Array.Empty<string>());

        var tagged = TagService.GetTaggedElementsByTagGroup("default").ToArray();
        Assert.AreEqual(1, tagged.Length);
        Assert.AreEqual(element1.Id, tagged[0].EntityId);
    }

    [Test]
    public async Task GetAllElementTags_Returns_Element_Tags_With_NodeCount()
    {
        IContentType elementType = await CreateElementTypeWithTags();

        IElement element1 = new Element("Tagged element 1", elementType);
        element1.AssignTags(PropertyEditorCollection, DataTypeService, IdKeyMap, Serializer, "tags", new[] { "cow", "pig", "goat" });
        ElementService.Save(element1);
        ElementService.Publish(element1, Array.Empty<string>());

        IElement element2 = new Element("Tagged element 2", elementType);
        element2.AssignTags(PropertyEditorCollection, DataTypeService, IdKeyMap, Serializer, "tags", new[] { "cow", "pig" });
        ElementService.Save(element2);
        ElementService.Publish(element2, Array.Empty<string>());

        IElement element3 = new Element("Tagged element 3", elementType);
        element3.AssignTags(PropertyEditorCollection, DataTypeService, IdKeyMap, Serializer, "tags", new[] { "cow" });
        ElementService.Save(element3);
        ElementService.Publish(element3, Array.Empty<string>());

        var tags = TagService.GetAllElementTags()
            .OrderByDescending(x => x.NodeCount)
            .ToList();

        Assert.AreEqual(3, tags.Count);
        Assert.AreEqual("cow", tags[0].Text);
        Assert.AreEqual(3, tags[0].NodeCount);
        Assert.AreEqual("pig", tags[1].Text);
        Assert.AreEqual(2, tags[1].NodeCount);
        Assert.AreEqual("goat", tags[2].Text);
        Assert.AreEqual(1, tags[2].NodeCount);
    }

    [Test]
    public async Task Element_And_Content_Tags_Are_Isolated_By_ObjectType()
    {
        IContentType elementType = await CreateElementTypeWithTags();

        IContent content = ContentBuilder.CreateSimpleContent(_contentType, "Tagged content");
        content.AssignTags(PropertyEditorCollection, DataTypeService, IdKeyMap, Serializer, "tags", new[] { "shared" });
        ContentService.Save(content);
        ContentService.Publish(content, Array.Empty<string>());

        IElement element = new Element("Tagged element", elementType);
        element.AssignTags(PropertyEditorCollection, DataTypeService, IdKeyMap, Serializer, "tags", new[] { "shared" });
        ElementService.Save(element);
        ElementService.Publish(element, Array.Empty<string>());

        var taggedContent = TagService.GetTaggedContentByTag("shared").ToArray();
        Assert.AreEqual(1, taggedContent.Length);
        Assert.AreEqual(content.Id, taggedContent[0].EntityId);

        var taggedElements = TagService.GetTaggedElementsByTag("shared").ToArray();
        Assert.AreEqual(1, taggedElements.Length);
        Assert.AreEqual(element.Id, taggedElements[0].EntityId);
    }

    [Test]
    public void TagList_Contains_NodeCount()
    {
        var content1 = ContentBuilder.CreateSimpleContent(_contentType, "Tagged content 1");
        content1.AssignTags(
            PropertyEditorCollection,
            DataTypeService,
            IdKeyMap,
            Serializer,
            "tags",
            new[] { "cow", "pig", "goat" });
        ContentService.Save(content1);
        ContentService.Publish(content1, Array.Empty<string>());

        var content2 = ContentBuilder.CreateSimpleContent(_contentType, "Tagged content 2");
        content2.AssignTags(PropertyEditorCollection, DataTypeService, IdKeyMap, Serializer, "tags", new[] { "cow", "pig" });
        ContentService.Save(content2);
        ContentService.Publish(content2, Array.Empty<string>());

        var content3 = ContentBuilder.CreateSimpleContent(_contentType, "Tagged content 3");
        content3.AssignTags(PropertyEditorCollection, DataTypeService, IdKeyMap, Serializer, "tags", new[] { "cow" });
        ContentService.Save(content3);
        ContentService.Publish(content3, Array.Empty<string>());

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

    private async Task<IContentType> CreateElementTypeWithTags()
    {
        IContentType elementType = ContentTypeBuilder.CreateSimpleElementType("umbElement", "Mandatory Element Type");
        elementType.PropertyGroups.First().PropertyTypes.Add(
            new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "tags")
            {
                DataTypeId = Constants.DataTypes.Tags
            });
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        return elementType;
    }
}
