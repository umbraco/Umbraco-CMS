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
    public void Can_Use_Markup_String_As_Value()
    {
        var contentType = ContentTypeBuilder.CreateTextPageContentType("myContentType");
        contentType.AllowedTemplates = Enumerable.Empty<ITemplate>();
        ContentTypeService.Save(contentType);

        var dataType = DataTypeService.GetDataType(contentType.PropertyTypes.First(propertyType => propertyType.Alias == "bodyText").DataTypeId)!;
        var editor = dataType.Editor!;
        var valueEditor = editor.GetValueEditor();

        const string markup = "<p>This is some markup</p>";

        var content = ContentBuilder.CreateTextpageContent(contentType, "My Content", -1);
        content.Properties["bodyText"]!.SetValue(markup);
        ContentService.Save(content);

        var toEditor = valueEditor.ToEditor(content.Properties["bodyText"]);
        var richTextEditorValue = toEditor as RichTextEditorValue;

        Assert.IsNotNull(richTextEditorValue);
        Assert.AreEqual(markup, richTextEditorValue.Markup);
    }

    [Test]
    public void Can_Use_RichTextEditorValue_As_Value()
    {
        var contentType = ContentTypeBuilder.CreateTextPageContentType("myContentType");
        contentType.AllowedTemplates = Enumerable.Empty<ITemplate>();
        ContentTypeService.Save(contentType);

        var dataType = DataTypeService.GetDataType(contentType.PropertyTypes.First(propertyType => propertyType.Alias == "bodyText").DataTypeId)!;
        var editor = dataType.Editor!;
        var valueEditor = editor.GetValueEditor();

        const string markup = "<p>This is some markup</p>";
        var propertyValue = RichTextPropertyEditorHelper.SerializeRichTextEditorValue(new RichTextEditorValue { Markup = markup, Blocks = null }, JsonSerializer);

        var content = ContentBuilder.CreateTextpageContent(contentType, "My Content", -1);
        content.Properties["bodyText"]!.SetValue(propertyValue);
        ContentService.Save(content);

        var toEditor = valueEditor.ToEditor(content.Properties["bodyText"]);
        var richTextEditorValue = toEditor as RichTextEditorValue;

        Assert.IsNotNull(richTextEditorValue);
        Assert.AreEqual(markup, richTextEditorValue.Markup);
    }

    [Test]
    public void Can_Track_Block_References()
    {
        var elementType = ContentTypeBuilder.CreateAllTypesContentType("myElementType", "My Element Type");
        elementType.IsElement = true;
        ContentTypeService.Save(elementType);

        var contentType = ContentTypeBuilder.CreateTextPageContentType("myContentType");
        contentType.AllowedTemplates = Enumerable.Empty<ITemplate>();
        ContentTypeService.Save(contentType);

        var pickedContent = ContentBuilder.CreateTextpageContent(contentType, "My Content", -1);
        ContentService.Save(pickedContent);

        var dataType = DataTypeService.GetDataType(contentType.PropertyTypes.First(propertyType => propertyType.Alias == "bodyText").DataTypeId)!;
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
        Assert.AreEqual(1, references.Length);
        var reference = references.First();
        Assert.AreEqual(Constants.Conventions.RelationTypes.RelatedDocumentAlias, reference.RelationTypeAlias);
        Assert.AreEqual(pickedContent.GetUdi(), reference.Udi);
    }

    [Test]
    public void Can_Track_Block_Tags()
    {
        var elementType = ContentTypeBuilder.CreateAllTypesContentType("myElementType", "My Element Type");
        elementType.IsElement = true;
        ContentTypeService.Save(elementType);

        var contentType = ContentTypeBuilder.CreateTextPageContentType("myContentType");
        contentType.AllowedTemplates = Enumerable.Empty<ITemplate>();
        ContentTypeService.Save(contentType);

        var dataType = DataTypeService.GetDataType(contentType.PropertyTypes.First(propertyType => propertyType.Alias == "bodyText").DataTypeId)!;
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
        Assert.AreEqual(3, tags.Length);
        Assert.IsNotNull(tags.Single(tag => tag.Text == "Tag One"));
        Assert.IsNotNull(tags.Single(tag => tag.Text == "Tag Two"));
        Assert.IsNotNull(tags.Single(tag => tag.Text == "Tag Three"));
    }

    [TestCase(null, false)]
    [TestCase("", false)]
    [TestCase("""{"markup":"","blocks":null}""", false)]
    [TestCase("""{"markup":"<p></p>","blocks":null}""", false)]
    [TestCase("abc", true)]
    [TestCase("""{"markup":"abc","blocks":null}""", true)]
    public async Task Can_Handle_Empty_Value_Representations(string? rteValue, bool expectedIsValue)
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("myPage")
            .WithName("My Page")
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
            .Done()
            .Done()
            .Build();

        var contentTypeResult = await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        Assert.IsTrue(contentTypeResult.Success);

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
        Assert.IsTrue(contentResult.Success);

        var publishResult = ContentService.Publish(content, []);
        Assert.IsTrue(publishResult.Success);

        var publishedContent = await PublishedContentCache.GetByIdAsync(content.Key);
        Assert.IsNotNull(publishedContent);

        var publishedProperty = publishedContent.Properties.First(property => property.Alias == "rte");
        Assert.AreEqual(expectedIsValue, publishedProperty.HasValue());

        Assert.AreEqual(expectedIsValue, publishedContent.HasValue("rte"));
    }
}
