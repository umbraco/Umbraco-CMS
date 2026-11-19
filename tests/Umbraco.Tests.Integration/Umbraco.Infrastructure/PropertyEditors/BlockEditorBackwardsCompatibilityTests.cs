using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class BlockEditorBackwardsCompatibilityTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    [TestCase]
    public async Task RichTextWithPropertyLessBlocksIsBackwardsCompatible()
    {
        var elementType = await CreatePropertyLessElementType();
        var richTextDataType = await CreateRichTextDataType(elementType);
        var contentType = await CreateContentType(richTextDataType);

        // Legacy data missing the "expose" property (#23379). The layout uses "contentKey" because the
        // obsolete "contentUdi" layout format was removed in Umbraco 18; contentData still carries the
        // legacy "udi" identifier, which remains supported on read.
        var json = $$"""
                     {
                         "markup": "<p><umb-rte-block data-content-udi=\"umb://element/1304e1ddac87439684fe8a399231cb3d\"></umb-rte-block></p>",
                         "blocks": {
                             "layout": {
                                 "{{Constants.PropertyEditors.Aliases.RichText}}": [
                                     {
                                         "contentKey": "1304e1dd-ac87-4396-84fe-8a399231cb3d"
                                     }
                                 ]
                             },
                             "contentData": [
                                 {
                                     "contentTypeKey": "{{elementType.Key}}",
                                     "udi": "umb://element/1304e1ddac87439684fe8a399231cb3d"
                                 }
                             ],
                             "settingsData": []
                         }
                     }
                     """;

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Home");

        var content = contentBuilder.Build();
        content.Properties["blocks"]!.SetValue(json);
        ContentService.Save(content);

        var toEditor = richTextDataType.Editor!.GetValueEditor().ToEditor(content.Properties["blocks"]!) as RichTextEditorValue;
        Assert.IsNotNull(toEditor);
        Assert.IsNotNull(toEditor.Blocks);

        Assert.AreEqual(1, toEditor.Blocks.ContentData.Count);
        Assert.AreEqual("1304e1ddac87439684fe8a399231cb3d", toEditor.Blocks.ContentData[0].Key.ToString("N"));
        Assert.IsEmpty(toEditor.Blocks.ContentData[0].Values);

        // The block has no properties, so historically it was never exposed on upgrade and rendered
        // as unpublished (#23379). It must now be exposed to preserve its published state.
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, toEditor.Blocks.Expose.Count);
            Assert.AreEqual("1304e1ddac87439684fe8a399231cb3d", toEditor.Blocks.Expose[0].ContentKey.ToString("N"));
        });
    }

    private static void AssertValueEquals(BlockItemData blockItemData, string propertyAlias, string expectedValue)
    {
        var blockPropertyValue = blockItemData.Values.FirstOrDefault(v => v.Alias == propertyAlias);
        Assert.IsNotNull(blockPropertyValue);
        Assert.AreEqual(expectedValue, blockPropertyValue.Value);
    }

    private async Task<IContentType> CreatePropertyLessElementType()
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias("myPropertyLessElementType")
            .WithName("My Property-less Element Type")
            .WithIsElement(true)
            .Build();

        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        return elementType;
    }

    private async Task<IDataType> CreateRichTextDataType(IContentType elementType)
    {
        var dataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.RichText], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new RichTextConfiguration.RichTextBlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementType.Key, SettingsElementTypeKey = elementType.Key }
                    }
                }
            },
            Name = "My Rich Text",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return dataType;
    }

    private async Task<IContentType> CreateContentType(IDataType blockEditorDataType)
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("myPage")
            .WithName("My Page")
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("Blocks")
            .WithDataTypeId(blockEditorDataType.Id)
            .Done()
            .Build();

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return contentType;
    }
}
