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
    public async Task BlockListIsBackwardsCompatible()
    {
        var elementType = await CreateElementType();
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(blockListDataType);

        var json = $$"""
                     {
                         "layout": {
                             "{{Constants.PropertyEditors.Aliases.BlockList}}": [
                                 {
                                     "contentUdi": "umb://element/1304e1ddac87439684fe8a399231cb3d",
                                     "settingsUdi": "umb://element/1f613e26ce274898908a561437af5100"
                                 },
                                 {
                                     "contentUdi": "umb://element/0a4a416e547d464fabcc6f345c17809a",
                                     "settingsUdi": "umb://element/63027539b0db45e7b70459762d4e83dd"
                                 }
                             ]
                         },
                         "contentData": [
                             {
                                 "contentTypeKey": "{{elementType.Key}}",
                                 "udi": "umb://element/1304e1ddac87439684fe8a399231cb3d",
                                 "title": "Content Title One",
                                 "text": "Content Text One"
                             },
                             {
                                 "contentTypeKey": "{{elementType.Key}}",
                                 "udi": "umb://element/0a4a416e547d464fabcc6f345c17809a",
                                 "title": "Content Title Two",
                                 "text": "Content Text Two"
                             }
                         ],
                         "settingsData": [
                             {
                                 "contentTypeKey": "{{elementType.Key}}",
                                 "udi": "umb://element/1f613e26ce274898908a561437af5100",
                                 "title": "Settings Title One",
                                 "text": "Settings Text One"
                             },
                             {
                                 "contentTypeKey": "{{elementType.Key}}",
                                 "udi": "umb://element/63027539b0db45e7b70459762d4e83dd",
                                 "title": "Settings Title Two",
                                 "text": "Settings Text Two"
                             }
                         ]
                     }
                     """;

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Home");

        var content = contentBuilder.Build();
        content.Properties["blocks"]!.SetValue(json);
        ContentService.Save(content);

        var toEditor = blockListDataType.Editor!.GetValueEditor().ToEditor(content.Properties["blocks"]!) as BlockListValue;
        Assert.IsNotNull(toEditor);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, toEditor.ContentData.Count);

            Assert.AreEqual("1304e1ddac87439684fe8a399231cb3d", toEditor.ContentData[0].Key.ToString("N"));
            Assert.AreEqual("0a4a416e547d464fabcc6f345c17809a", toEditor.ContentData[1].Key.ToString("N"));

            AssertValueEquals(toEditor.ContentData[0], "title", "Content Title One");
            AssertValueEquals(toEditor.ContentData[0], "text", "Content Text One");
            AssertValueEquals(toEditor.ContentData[1], "title", "Content Title Two");
            AssertValueEquals(toEditor.ContentData[1], "text", "Content Text Two");

            Assert.IsFalse(toEditor.ContentData[0].RawPropertyValues.Any());
            Assert.IsFalse(toEditor.ContentData[1].RawPropertyValues.Any());
        });

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, toEditor.SettingsData.Count);

            Assert.AreEqual("1f613e26ce274898908a561437af5100", toEditor.SettingsData[0].Key.ToString("N"));
            Assert.AreEqual("63027539b0db45e7b70459762d4e83dd", toEditor.SettingsData[1].Key.ToString("N"));

            AssertValueEquals(toEditor.SettingsData[0], "title", "Settings Title One");
            AssertValueEquals(toEditor.SettingsData[0], "text", "Settings Text One");
            AssertValueEquals(toEditor.SettingsData[1], "title", "Settings Title Two");
            AssertValueEquals(toEditor.SettingsData[1], "text", "Settings Text Two");

            Assert.IsFalse(toEditor.SettingsData[0].RawPropertyValues.Any());
            Assert.IsFalse(toEditor.SettingsData[1].RawPropertyValues.Any());
        });

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, toEditor.Expose.Count);

            Assert.AreEqual("1304e1ddac87439684fe8a399231cb3d", toEditor.Expose[0].ContentKey.ToString("N"));
            Assert.AreEqual("0a4a416e547d464fabcc6f345c17809a", toEditor.Expose[1].ContentKey.ToString("N"));
        });
    }

    [TestCase]
    public async Task BlockGridIsBackwardsCompatible()
    {
        var elementType = await CreateElementType();
        var gridAreaKey = Guid.NewGuid();
        var blockGridDataType = await CreateBlockGridDataType(elementType, gridAreaKey);
        var contentType = await CreateContentType(blockGridDataType);

        var json = $$"""
                     {
                         "layout": {
                             "{{Constants.PropertyEditors.Aliases.BlockGrid}}": [
                                 {
                                     "contentUdi": "umb://element/1304e1ddac87439684fe8a399231cb3d",
                                     "settingsUdi": "umb://element/1f613e26ce274898908a561437af5100",
                                     "columnSpan": 12,
                                     "rowSpan": 1,
                                     "areas": [{
                                        "key": "{{gridAreaKey}}",
                                        "items": [{
                                           "contentUdi": "umb://element/5fc866c590be4d01a28a979472a1ffee",
                                           "areas": [],
                                           "columnSpan": 12,
                                           "rowSpan": 1
                                        }]
                                    }]
                                 },
                                 {
                                     "contentUdi": "umb://element/0a4a416e547d464fabcc6f345c17809a",
                                     "settingsUdi": "umb://element/63027539b0db45e7b70459762d4e83dd",
                                     "columnSpan": 12,
                                     "rowSpan": 1,
                                     "areas": [{
                                        "key": "{{gridAreaKey}}",
                                        "items": [{
                                           "contentUdi": "umb://element/264536b65b0f4641aa43d4bfb515831d",
                                           "areas": [],
                                           "columnSpan": 12,
                                           "rowSpan": 1
                                        }]
                                    }]
                                 }
                             ]
                         },
                         "contentData": [
                             {
                                 "contentTypeKey": "{{elementType.Key}}",
                                 "udi": "umb://element/1304e1ddac87439684fe8a399231cb3d",
                                 "title": "Content Title One",
                                 "text": "Content Text One"
                             },
                             {
                                 "contentTypeKey": "{{elementType.Key}}",
                                 "udi": "umb://element/0a4a416e547d464fabcc6f345c17809a",
                                 "title": "Content Title Two",
                                 "text": "Content Text Two"
                             },
                             {
                                 "contentTypeKey": "{{elementType.Key}}",
                                 "udi": "umb://element/5fc866c590be4d01a28a979472a1ffee",
                                 "title": "Content Area Title One",
                                 "text": "Content Area Text One"
                             },
                             {
                                 "contentTypeKey": "{{elementType.Key}}",
                                 "udi": "umb://element/264536b65b0f4641aa43d4bfb515831d",
                                 "title": "Content Area Title Two",
                                 "text": "Content Area Text Two"
                             }
                         ],
                         "settingsData": [
                             {
                                 "contentTypeKey": "{{elementType.Key}}",
                                 "udi": "umb://element/1f613e26ce274898908a561437af5100",
                                 "title": "Settings Title One",
                                 "text": "Settings Text One"
                             },
                             {
                                 "contentTypeKey": "{{elementType.Key}}",
                                 "udi": "umb://element/63027539b0db45e7b70459762d4e83dd",
                                 "title": "Settings Title Two",
                                 "text": "Settings Text Two"
                             }
                         ]
                     }
                     """;

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Home");

        var content = contentBuilder.Build();
        content.Properties["blocks"]!.SetValue(json);
        ContentService.Save(content);

        var toEditor = blockGridDataType.Editor!.GetValueEditor().ToEditor(content.Properties["blocks"]!) as BlockGridValue;
        Assert.IsNotNull(toEditor);

        Assert.AreEqual(4, toEditor.ContentData.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("1304e1ddac87439684fe8a399231cb3d", toEditor.ContentData[0].Key.ToString("N"));
            Assert.AreEqual("0a4a416e547d464fabcc6f345c17809a", toEditor.ContentData[1].Key.ToString("N"));
            Assert.AreEqual("5fc866c590be4d01a28a979472a1ffee", toEditor.ContentData[2].Key.ToString("N"));
            Assert.AreEqual("264536b65b0f4641aa43d4bfb515831d", toEditor.ContentData[3].Key.ToString("N"));

            AssertValueEquals(toEditor.ContentData[0], "title", "Content Title One");
            AssertValueEquals(toEditor.ContentData[0], "text", "Content Text One");
            AssertValueEquals(toEditor.ContentData[1], "title", "Content Title Two");
            AssertValueEquals(toEditor.ContentData[1], "text", "Content Text Two");
            AssertValueEquals(toEditor.ContentData[2], "title", "Content Area Title One");
            AssertValueEquals(toEditor.ContentData[2], "text", "Content Area Text One");
            AssertValueEquals(toEditor.ContentData[3], "title", "Content Area Title Two");
            AssertValueEquals(toEditor.ContentData[3], "text", "Content Area Text Two");

            Assert.IsFalse(toEditor.ContentData[0].RawPropertyValues.Any());
            Assert.IsFalse(toEditor.ContentData[1].RawPropertyValues.Any());
            Assert.IsFalse(toEditor.ContentData[2].RawPropertyValues.Any());
            Assert.IsFalse(toEditor.ContentData[3].RawPropertyValues.Any());
        });

        Assert.AreEqual(2, toEditor.SettingsData.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("1f613e26ce274898908a561437af5100", toEditor.SettingsData[0].Key.ToString("N"));
            Assert.AreEqual("63027539b0db45e7b70459762d4e83dd", toEditor.SettingsData[1].Key.ToString("N"));

            AssertValueEquals(toEditor.SettingsData[0], "title", "Settings Title One");
            AssertValueEquals(toEditor.SettingsData[0], "text", "Settings Text One");
            AssertValueEquals(toEditor.SettingsData[1], "title", "Settings Title Two");
            AssertValueEquals(toEditor.SettingsData[1], "text", "Settings Text Two");

            Assert.IsFalse(toEditor.SettingsData[0].RawPropertyValues.Any());
            Assert.IsFalse(toEditor.SettingsData[1].RawPropertyValues.Any());
        });

        Assert.Multiple(() =>
        {
            Assert.AreEqual(4, toEditor.Expose.Count);

            Assert.AreEqual("1304e1ddac87439684fe8a399231cb3d", toEditor.Expose[0].ContentKey.ToString("N"));
            Assert.AreEqual("0a4a416e547d464fabcc6f345c17809a", toEditor.Expose[1].ContentKey.ToString("N"));
            Assert.AreEqual("5fc866c590be4d01a28a979472a1ffee", toEditor.Expose[2].ContentKey.ToString("N"));
            Assert.AreEqual("264536b65b0f4641aa43d4bfb515831d", toEditor.Expose[3].ContentKey.ToString("N"));
        });
    }

    [TestCase]
    public async Task RichTextIsBackwardsCompatible()
    {
        var elementType = await CreateElementType();
        var richTextDataType = await CreateRichTextDataType(elementType);
        var contentType = await CreateContentType(richTextDataType);

        var json = $$"""
                     {
                         "markup": "<p>huh?</p>",
                         "blocks": {
                             "layout": {
                                 "{{Constants.PropertyEditors.Aliases.TinyMce}}": [
                                     {
                                         "contentUdi": "umb://element/1304e1ddac87439684fe8a399231cb3d",
                                         "settingsUdi": "umb://element/1f613e26ce274898908a561437af5100"
                                     },
                                     {
                                         "contentUdi": "umb://element/0a4a416e547d464fabcc6f345c17809a",
                                         "settingsUdi": "umb://element/63027539b0db45e7b70459762d4e83dd"
                                     }
                                 ]
                             },
                             "contentData": [
                                 {
                                     "contentTypeKey": "{{elementType.Key}}",
                                     "udi": "umb://element/1304e1ddac87439684fe8a399231cb3d",
                                     "title": "Content Title One",
                                     "text": "Content Text One"
                                 },
                                 {
                                     "contentTypeKey": "{{elementType.Key}}",
                                     "udi": "umb://element/0a4a416e547d464fabcc6f345c17809a",
                                     "title": "Content Title Two",
                                     "text": "Content Text Two"
                                 }
                             ],
                             "settingsData": [
                                 {
                                     "contentTypeKey": "{{elementType.Key}}",
                                     "udi": "umb://element/1f613e26ce274898908a561437af5100",
                                     "title": "Settings Title One",
                                     "text": "Settings Text One"
                                 },
                                 {
                                     "contentTypeKey": "{{elementType.Key}}",
                                     "udi": "umb://element/63027539b0db45e7b70459762d4e83dd",
                                     "title": "Settings Title Two",
                                     "text": "Settings Text Two"
                                 }
                             ]
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

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, toEditor.Blocks.ContentData.Count);

            Assert.AreEqual("1304e1ddac87439684fe8a399231cb3d", toEditor.Blocks.ContentData[0].Key.ToString("N"));
            Assert.AreEqual("0a4a416e547d464fabcc6f345c17809a", toEditor.Blocks.ContentData[1].Key.ToString("N"));

            AssertValueEquals(toEditor.Blocks.ContentData[0], "title", "Content Title One");
            AssertValueEquals(toEditor.Blocks.ContentData[0], "text", "Content Text One");
            AssertValueEquals(toEditor.Blocks.ContentData[1], "title", "Content Title Two");
            AssertValueEquals(toEditor.Blocks.ContentData[1], "text", "Content Text Two");

            Assert.IsFalse(toEditor.Blocks.ContentData[0].RawPropertyValues.Any());
            Assert.IsFalse(toEditor.Blocks.ContentData[1].RawPropertyValues.Any());
        });

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, toEditor.Blocks.SettingsData.Count);

            Assert.AreEqual("1f613e26ce274898908a561437af5100", toEditor.Blocks.SettingsData[0].Key.ToString("N"));
            Assert.AreEqual("63027539b0db45e7b70459762d4e83dd", toEditor.Blocks.SettingsData[1].Key.ToString("N"));

            AssertValueEquals(toEditor.Blocks.SettingsData[0], "title", "Settings Title One");
            AssertValueEquals(toEditor.Blocks.SettingsData[0], "text", "Settings Text One");
            AssertValueEquals(toEditor.Blocks.SettingsData[1], "title", "Settings Title Two");
            AssertValueEquals(toEditor.Blocks.SettingsData[1], "text", "Settings Text Two");

            Assert.IsFalse(toEditor.Blocks.SettingsData[0].RawPropertyValues.Any());
            Assert.IsFalse(toEditor.Blocks.SettingsData[1].RawPropertyValues.Any());
        });

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, toEditor.Blocks.Expose.Count);

            Assert.AreEqual("1304e1ddac87439684fe8a399231cb3d", toEditor.Blocks.Expose[0].ContentKey.ToString("N"));
            Assert.AreEqual("0a4a416e547d464fabcc6f345c17809a", toEditor.Blocks.Expose[1].ContentKey.ToString("N"));
        });
    }

    private static void AssertValueEquals(BlockItemData blockItemData, string propertyAlias, string expectedValue)
    {
        var blockPropertyValue = blockItemData.Values.FirstOrDefault(v => v.Alias == propertyAlias);
        Assert.IsNotNull(blockPropertyValue);
        Assert.AreEqual(expectedValue, blockPropertyValue.Value);
    }

    private async Task<IContentType> CreateElementType()
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias("myElementType")
            .WithName("My Element Type")
            .WithIsElement(true)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .Done()
            .AddPropertyType()
            .WithAlias("text")
            .WithName("Text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .Done()
            .Build();

        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        return elementType;
    }

    private async Task<IDataType> CreateBlockListDataType(IContentType elementType)
    {
        var dataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementType.Key, SettingsElementTypeKey = elementType.Key }
                    }
                }
            },
            Name = "My Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return dataType;
    }

    private async Task<IDataType> CreateBlockGridDataType(IContentType elementType, Guid gridAreaKey)
    {
        var dataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockGrid], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockGridConfiguration.BlockGridBlockConfiguration[]
                    {
                        new()
                        {
                            ContentElementTypeKey = elementType.Key,
                            SettingsElementTypeKey = elementType.Key,
                            AllowInAreas = true,
                            AllowAtRoot = true,
                            Areas =
                            [
                                new BlockGridConfiguration.BlockGridAreaConfiguration
                                {
                                    Key = gridAreaKey,
                                    Alias = "areaOne"
                                }
                            ]
                        }
                    }
                }
            },
            Name = "My Block Grid",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return dataType;
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
