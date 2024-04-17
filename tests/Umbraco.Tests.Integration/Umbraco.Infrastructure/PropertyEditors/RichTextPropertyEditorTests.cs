using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class RichTextPropertyEditorTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

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
        var valueEditor = (BlockValuePropertyValueEditorBase)editor.GetValueEditor();

        var elementId = Guid.NewGuid();
        var propertyValue = RichTextPropertyEditorHelper.SerializeRichTextEditorValue(
            new RichTextEditorValue
            {
                Markup = @$"<p>This is some markup</p><umb-rte-block data-content-udi=""umb://element/{elementId:N}""><!--Umbraco-Block--></umb-rte-block>",
                Blocks = JsonSerializer.Deserialize<BlockValue>($$"""
                                                                  {
                                                                  	"layout": {
                                                                  		"Umbraco.TinyMCE": [{
                                                                  				"contentUdi": "umb://element/{{elementId:N}}"
                                                                  			}
                                                                  		]
                                                                  	},
                                                                  	"contentData": [{
                                                                  			"contentTypeKey": "{{elementType.Key:B}}",
                                                                  			"udi": "umb://element/{{elementId:N}}",
                                                                  			"contentPicker": "umb://document/{{pickedContent.Key:N}}"
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
        var valueEditor = (BlockValuePropertyValueEditorBase)editor.GetValueEditor();

        var elementId = Guid.NewGuid();
        var propertyValue = RichTextPropertyEditorHelper.SerializeRichTextEditorValue(
            new RichTextEditorValue
            {
                Markup = @$"<p>This is some markup</p><umb-rte-block data-content-udi=""umb://element/{elementId:N}""><!--Umbraco-Block--></umb-rte-block>",
                Blocks = JsonSerializer.Deserialize<BlockValue>($$"""
                                                                  {
                                                                  	"layout": {
                                                                  		"Umbraco.TinyMCE": [{
                                                                  				"contentUdi": "umb://element/{{elementId:N}}"
                                                                  			}
                                                                  		]
                                                                  	},
                                                                  	"contentData": [{
                                                                  			"contentTypeKey": "{{elementType.Key:B}}",
                                                                  			"udi": "umb://element/{{elementId:N}}",
                                                                  			"tags": "['Tag One', 'Tag Two', 'Tag Three']"
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
}
