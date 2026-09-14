using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class RichTextPropertyEditorTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IPublishedContentCache PublishedContentCache => GetRequiredService<IPublishedContentCache>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    [Test]
    public async Task Can_Use_Markup_String_As_Value()
    {
        var contentType = ContentTypeBuilder.CreateTextPageContentType("myContentType");
        contentType.AllowedTemplates = Enumerable.Empty<ITemplate>();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var dataTypeId = contentType.PropertyTypes.First(propertyType => propertyType.Alias == "bodyText").DataTypeId;
        var keyAttempt = IdKeyMap.GetKeyForId(dataTypeId, UmbracoObjectTypes.DataType);
        Assert.That(keyAttempt.Success, Is.True, $"Could not resolve a GUID key for data type id {dataTypeId}.");
        var dataType = (await DataTypeService.GetAsync(keyAttempt.Result))!;
        var editor = dataType.Editor!;
        var valueEditor = editor.GetValueEditor();

        const string markup = "<p>This is some markup</p>";

        var content = ContentBuilder.CreateTextpageContent(contentType, "My Content", -1);
        content.Properties["bodyText"]!.SetValue(markup);
        ContentService.Save(content);

        var toEditor = valueEditor.ToEditor(content.Properties["bodyText"]);
        var richTextEditorValue = toEditor as RichTextEditorValue;

        Assert.That(richTextEditorValue, Is.Not.Null);
        Assert.That(richTextEditorValue.Markup, Is.EqualTo(markup));
    }

    [Test]
    public async Task Can_Use_RichTextEditorValue_As_Value()
    {
        var contentType = ContentTypeBuilder.CreateTextPageContentType("myContentType");
        contentType.AllowedTemplates = Enumerable.Empty<ITemplate>();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var dataTypeId = contentType.PropertyTypes.First(propertyType => propertyType.Alias == "bodyText").DataTypeId;
        var keyAttempt = IdKeyMap.GetKeyForId(dataTypeId, UmbracoObjectTypes.DataType);
        Assert.That(keyAttempt.Success, Is.True, $"Could not resolve a GUID key for data type id {dataTypeId}.");
        var dataType = (await DataTypeService.GetAsync(keyAttempt.Result))!;
        var editor = dataType.Editor!;
        var valueEditor = editor.GetValueEditor();

        const string markup = "<p>This is some markup</p>";
        var propertyValue = RichTextPropertyEditorHelper.SerializeRichTextEditorValue(new RichTextEditorValue { Markup = markup, Blocks = null }, JsonSerializer);

        var content = ContentBuilder.CreateTextpageContent(contentType, "My Content", -1);
        content.Properties["bodyText"]!.SetValue(propertyValue);
        ContentService.Save(content);

        var toEditor = valueEditor.ToEditor(content.Properties["bodyText"]);
        var richTextEditorValue = toEditor as RichTextEditorValue;

        Assert.That(richTextEditorValue, Is.Not.Null);
        Assert.That(richTextEditorValue.Markup, Is.EqualTo(markup));
    }

    [Test]
    public async Task Can_Track_Block_References()
    {
        var elementType = ContentTypeBuilder.CreateAllTypesContentType("myElementType", "My Element Type");
        elementType.IsElement = true;
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType("myContentType");
        contentType.AllowedTemplates = Enumerable.Empty<ITemplate>();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var pickedContent = ContentBuilder.CreateTextpageContent(contentType, "My Content", -1);
        ContentService.Save(pickedContent);

        var dataTypeId = contentType.PropertyTypes.First(propertyType => propertyType.Alias == "bodyText").DataTypeId;
        var keyAttempt = IdKeyMap.GetKeyForId(dataTypeId, UmbracoObjectTypes.DataType);
        Assert.That(keyAttempt.Success, Is.True, $"Could not resolve a GUID key for data type id {dataTypeId}.");
        var dataType = (await DataTypeService.GetAsync(keyAttempt.Result))!;
        var editor = dataType.Editor!;
        var valueEditor = (BlockValuePropertyValueEditorBase<RichTextBlockValue, RichTextBlockLayoutItem>)editor.GetValueEditor();

        var elementId = Guid.NewGuid();
        var propertyValue = RichTextPropertyEditorHelper.SerializeRichTextEditorValue(
            new RichTextEditorValue
            {
                Markup = @$"<p>This is some markup</p><umb-rte-block data-content-key=""{elementId:D}""><!--Umbraco-Block--></umb-rte-block>",
                Blocks = JsonSerializer.Deserialize<RichTextBlockValue>($$"""
                                                                  {
                                                                  	"layout": {
                                                                  		"{{Constants.PropertyEditors.Aliases.RichText}}": [{
                                                                  				"contentKey": "{{elementId:D}}"
                                                                  			}
                                                                  		]
                                                                  	},
                                                                  	"contentData": [{
                                                                  			"contentTypeKey": "{{elementType.Key:D}}",
                                                                  			"key": "{{elementId:D}}",
                                                                  			"values": [
                                                                                { "alias": "contentPicker", "value": "umb://document/{{pickedContent.Key:N}}" }
                                                                  			]
                                                                  		}
                                                                  	],
                                                                  	"settingsData": []
                                                                  }
                                                                  """)
            },
            JsonSerializer);

        var content = ContentBuilder.CreateTextpageContent(contentType, "My Content", -1);
        content.Properties["bodyText"]!.SetValue(propertyValue);
        ContentService.Save(content);

        var references = valueEditor.GetReferences(content.GetValue("bodyText")).ToArray();
        Assert.That(references, Has.Length.EqualTo(1));
        var reference = references.First();
        Assert.That(reference.RelationTypeAlias, Is.EqualTo(Constants.Conventions.RelationTypes.RelatedDocumentAlias));
        Assert.That(reference.Udi, Is.EqualTo(pickedContent.GetUdi()));
    }

    [Test]
    public async Task Can_Track_Block_Tags()
    {
        var elementType = ContentTypeBuilder.CreateAllTypesContentType("myElementType", "My Element Type");
        elementType.IsElement = true;
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType("myContentType");
        contentType.AllowedTemplates = Enumerable.Empty<ITemplate>();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var dataTypeId = contentType.PropertyTypes.First(propertyType => propertyType.Alias == "bodyText").DataTypeId;
        var keyAttempt = IdKeyMap.GetKeyForId(dataTypeId, UmbracoObjectTypes.DataType);
        Assert.That(keyAttempt.Success, Is.True, $"Could not resolve a GUID key for data type id {dataTypeId}.");
        var dataType = (await DataTypeService.GetAsync(keyAttempt.Result))!;
        var editor = dataType.Editor!;
        var valueEditor = (BlockValuePropertyValueEditorBase<RichTextBlockValue, RichTextBlockLayoutItem>)editor.GetValueEditor();

        var elementId = Guid.NewGuid();
        var propertyValue = RichTextPropertyEditorHelper.SerializeRichTextEditorValue(
            new RichTextEditorValue
            {
                Markup = @$"<p>This is some markup</p><umb-rte-block data-content-key=""{elementId:D}""><!--Umbraco-Block--></umb-rte-block>",
                Blocks = JsonSerializer.Deserialize<RichTextBlockValue>($$"""
                                                                  {
                                                                  	"layout": {
                                                                  		"{{Constants.PropertyEditors.Aliases.RichText}}": [{
                                                                  				"contentKey": "{{elementId:D}}"
                                                                  			}
                                                                  		]
                                                                  	},
                                                                  	"contentData": [{
                                                                  			"contentTypeKey": "{{elementType.Key:D}}",
                                                                  			"key": "{{elementId:D}}",
                                                                  			"values": [
                                                                                { "alias": "tags", "value": "[\"Tag One\", \"Tag Two\", \"Tag Three\"]" }
                                                                  			]
                                                                  		}
                                                                  	],
                                                                  	"settingsData": []
                                                                  }
                                                                  """)
            },
            JsonSerializer);

        var content = ContentBuilder.CreateTextpageContent(contentType, "My Content", -1);
        content.Properties["bodyText"]!.SetValue(propertyValue);
        ContentService.Save(content);

        var tags = valueEditor.GetTags(content.GetValue("bodyText"), null, null).ToArray();
        Assert.That(tags, Has.Length.EqualTo(3));
        Assert.That(tags.Single(tag => tag.Text == "Tag One"), Is.Not.Null);
        Assert.That(tags.Single(tag => tag.Text == "Tag Two"), Is.Not.Null);
        Assert.That(tags.Single(tag => tag.Text == "Tag Three"), Is.Not.Null);
    }

    [TestCase(null, false)]
    [TestCase("", false)]
    [TestCase("""{"markup":"","blocks":null}""", false)]
    [TestCase("""{"markup":"<p></p>","blocks":null}""", false)]
    [TestCase("abc", true)]
    [TestCase("""{"markup":"abc","blocks":null}""", true)]
    public async Task Can_Handle_Empty_Value_Representations_For_Invariant_Content(string? rteValue, bool expectedHasValue)
    {
        var contentType = await CreateContentTypeForEmptyValueTests();

        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Page")
            .WithPropertyValues(
                new
                {
                    rte = rteValue
                })
            .Build();

        var contentResult = ContentService.Save(content);
        Assert.That(contentResult.Success, Is.True);

        var publishResult = ContentService.Publish(content, []);
        Assert.That(publishResult.Success, Is.True);

        var publishedContent = await PublishedContentCache.GetByIdAsync(content.Key);
        Assert.That(publishedContent, Is.Not.Null);

        var publishedProperty = publishedContent.Properties.First(property => property.Alias == "rte");
        Assert.That(publishedProperty.HasValue(), Is.EqualTo(expectedHasValue));

        Assert.That(publishedContent.HasValue("rte"), Is.EqualTo(expectedHasValue));
    }

    [TestCase(null, false)]
    [TestCase("", false)]
    [TestCase("""{"markup":"","blocks":null}""", false)]
    [TestCase("""{"markup":"<p></p>","blocks":null}""", false)]
    [TestCase("abc", true)]
    [TestCase("""{"markup":"abc","blocks":null}""", true)]
    public async Task Can_Handle_Empty_Value_Representations_For_Variant_Content(string? rteValue, bool expectedHasValue)
    {
        var contentType = await CreateContentTypeForEmptyValueTests(ContentVariation.Culture);

        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Page")
            .WithCultureName("en-US", "Page")
            .WithPropertyValues(
                new
                {
                    rte = rteValue
                },
                "en-US")
            .Build();

        var contentResult = ContentService.Save(content);
        Assert.That(contentResult.Success, Is.True);

        var publishResult = ContentService.Publish(content, ["en-US"]);
        Assert.That(publishResult.Success, Is.True);

        var publishedContent = await PublishedContentCache.GetByIdAsync(content.Key);
        Assert.That(publishedContent, Is.Not.Null);

        var publishedProperty = publishedContent.Properties.First(property => property.Alias == "rte");
        Assert.That(publishedProperty.HasValue("en-US"), Is.EqualTo(expectedHasValue));

        Assert.That(publishedContent.HasValue("rte", "en-US"), Is.EqualTo(expectedHasValue));
    }

    private async Task<IContentType> CreateContentTypeForEmptyValueTests(ContentVariation contentVariation = ContentVariation.Nothing)
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("myPage")
            .WithName("My Page")
            .WithContentVariation(contentVariation)
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithSupportsPublishing(true)
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.RichText)
                    .WithDataTypeId(Constants.DataTypes.RichtextEditor)
                    .WithValueStorageType(ValueStorageType.Ntext)
                    .WithAlias("rte")
                    .WithName("RTE")
                    .WithVariations(contentVariation)
                    .Done()
                .Done()
            .Build();

        var contentTypeResult = await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        Assert.That(contentTypeResult.Success, Is.True);

        return contentType;
    }
}
