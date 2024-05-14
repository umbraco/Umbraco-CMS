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
public class PropertyIndexValueFactoryTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    [Test]
    public void Can_Get_Index_Values_From_RichText_With_Blocks()
    {
        var elementType = ContentTypeBuilder.CreateAllTypesContentType("myElementType", "My Element Type");
        elementType.IsElement = true;
        ContentTypeService.Save(elementType);

        var contentType = ContentTypeBuilder.CreateTextPageContentType("myContentType");
        contentType.AllowedTemplates = Enumerable.Empty<ITemplate>();
        ContentTypeService.Save(contentType);

        var dataType = DataTypeService.GetDataType(contentType.PropertyTypes.First(propertyType => propertyType.Alias == "bodyText").DataTypeId)!;
        var editor = dataType.Editor!;

        var elementId = Guid.NewGuid();
        var propertyValue = RichTextPropertyEditorHelper.SerializeRichTextEditorValue(
            new RichTextEditorValue
            {
                Markup = @$"<p>This is some markup</p><umb-rte-block data-content-udi=""umb://element/{elementId:N}""><!--Umbraco-Block--></umb-rte-block>",
                Blocks = JsonSerializer.Deserialize<RichTextBlockValue>($$"""
                                                                  {
                                                                  	"layout": {
                                                                  		"Umbraco.TinyMCE": [{
                                                                  				"contentUdi": "umb://element/{{elementId:N}}"
                                                                  			}
                                                                  		]
                                                                  	},
                                                                  	"contentData": [{
                                                                  			"contentTypeKey": "{{elementType.Key:D}}",
                                                                  			"udi": "umb://element/{{elementId:N}}",
                                                                  			"singleLineText": "The single line of text in the block",
                                                                  			"bodyText": "<p>The body text in the block</p>"
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

        var indexValues = editor.PropertyIndexValueFactory.GetIndexValues(
            content.Properties["bodyText"]!,
            culture: null,
            segment: null,
            published: false,
            availableCultures: Enumerable.Empty<string>(),
            contentTypeDictionary: new Dictionary<Guid, IContentType>
            {
                { elementType.Key, elementType }, { contentType.Key, contentType }
            }).ToDictionary();

        Assert.IsTrue(indexValues.TryGetValue("bodyText", out var bodyTextIndexValues));

        Assert.AreEqual(1, bodyTextIndexValues.Count());
        var bodyTextIndexValue = bodyTextIndexValues.First() as string;
        Assert.IsNotNull(bodyTextIndexValue);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(bodyTextIndexValue.Contains("This is some markup"));
            Assert.IsTrue(bodyTextIndexValue.Contains("The single line of text in the block"));
            Assert.IsTrue(bodyTextIndexValue.Contains("The body text in the block"));
        });
    }

    [Test]
    public void Can_Get_Index_Values_From_RichText_Without_Blocks()
    {
        var contentType = ContentTypeBuilder.CreateTextPageContentType("myContentType");
        contentType.AllowedTemplates = Enumerable.Empty<ITemplate>();
        ContentTypeService.Save(contentType);

        var dataType = DataTypeService.GetDataType(contentType.PropertyTypes.First(propertyType => propertyType.Alias == "bodyText").DataTypeId)!;
        var editor = dataType.Editor!;

        var content = ContentBuilder.CreateTextpageContent(contentType, "My Content", -1);
        content.Properties["bodyText"]!.SetValue("<p>This is some markup</p>");
        ContentService.Save(content);

        var indexValues = editor.PropertyIndexValueFactory.GetIndexValues(
            content.Properties["bodyText"]!,
            culture: null,
            segment: null,
            published: false,
            availableCultures: Enumerable.Empty<string>(),
            contentTypeDictionary: new Dictionary<Guid, IContentType>
            {
                { contentType.Key, contentType }
            }).ToDictionary();

        Assert.IsTrue(indexValues.TryGetValue("bodyText", out var bodyTextIndexValues));

        Assert.AreEqual(1, bodyTextIndexValues.Count());
        var bodyTextIndexValue = bodyTextIndexValues.First() as string;
        Assert.IsNotNull(bodyTextIndexValue);
        Assert.IsTrue(bodyTextIndexValue.Contains("This is some markup"));
    }

    [Test]
    public async Task Can_Get_Index_Values_From_BlockList()
    {
        var elementType = ContentTypeBuilder.CreateAllTypesContentType("myElementType", "My Element Type");
        elementType.IsElement = true;
        ContentTypeService.Save(elementType);

        var dataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    ConfigurationEditorJsonSerializer.Serialize(new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementType.Key }
                    })
                }
            },
            Name = "My Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        var builder = new ContentTypeBuilder();
        var contentType = builder
            .WithAlias("myPage")
            .WithName("My Page")
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("Blocks")
            .WithDataTypeId(dataType.Id)
            .Done()
            .Build();
        ContentTypeService.Save(contentType);

        var editor = dataType.Editor!;

        var contentElementUdi = new GuidUdi(Constants.UdiEntityType.Element, Guid.NewGuid());
        var blockListValue = new BlockListValue(
        [
            new BlockListLayoutItem(contentElementUdi)
        ])
        {
            ContentData =
            [
                new(contentElementUdi, elementType.Key, elementType.Alias)
                {
                    RawPropertyValues = new Dictionary<string, object?>
                    {
                        {"singleLineText", "The single line of text in the block"},
                        {"bodyText", "<p>The body text in the block</p>"}
                    }
                }
            ],
            SettingsData = []
        };
        var propertyValue = JsonSerializer.Serialize(blockListValue);

        var content = ContentBuilder.CreateBasicContent(contentType);
        content.Properties["blocks"]!.SetValue(propertyValue);
        ContentService.Save(content);

        var indexValues = editor.PropertyIndexValueFactory.GetIndexValues(
            content.Properties["blocks"]!,
            culture: null,
            segment: null,
            published: false,
            availableCultures: Enumerable.Empty<string>(),
            contentTypeDictionary: new Dictionary<Guid, IContentType>
            {
                { elementType.Key, elementType }, { contentType.Key, contentType }
            }).ToDictionary();

        Assert.IsTrue(indexValues.TryGetValue("blocks", out var blocksIndexValues));

        Assert.AreEqual(1, blocksIndexValues.Count());
        var blockIndexValue = blocksIndexValues.First() as string;
        Assert.IsNotNull(blockIndexValue);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(blockIndexValue.Contains("The single line of text in the block"));
            Assert.IsTrue(blockIndexValue.Contains("The body text in the block"));
        });
    }

    [Test]
    public async Task Can_Get_Index_Values_From_BlockGrid()
    {
        var elementType = ContentTypeBuilder.CreateAllTypesContentType("myElementType", "My Element Type");
        elementType.IsElement = true;
        ContentTypeService.Save(elementType);

        var dataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockGrid], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    ConfigurationEditorJsonSerializer.Serialize(new BlockGridConfiguration.BlockGridBlockConfiguration[]
                    {
                        new()
                        {
                            ContentElementTypeKey = elementType.Key,
                            Areas = new BlockGridConfiguration.BlockGridAreaConfiguration[]
                            {
                                new()
                                {
                                   Key = Guid.NewGuid(),
                                   Alias = "one",
                                   ColumnSpan = 12,
                                   RowSpan = 1
                                }
                            }
                        }
                    })
                }
            },
            Name = "My Block Grid",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        var builder = new ContentTypeBuilder();
        var contentType = builder
            .WithAlias("myPage")
            .WithName("My Page")
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("Blocks")
            .WithDataTypeId(dataType.Id)
            .Done()
            .Build();
        ContentTypeService.Save(contentType);

        var editor = dataType.Editor!;

        var contentElementUdi = new GuidUdi(Constants.UdiEntityType.Element, Guid.NewGuid());
        var contentAreaElementUdi = new GuidUdi(Constants.UdiEntityType.Element, Guid.NewGuid());
        var blockGridValue = new BlockGridValue(
        [
            new BlockGridLayoutItem(contentElementUdi)
            {
                ColumnSpan = 12,
                RowSpan = 1,
                Areas =
                [
                    new BlockGridLayoutAreaItem(Guid.NewGuid())
                    {
                        Items =
                        [
                            new BlockGridLayoutItem(contentAreaElementUdi)
                            {
                                ColumnSpan = 12,
                                RowSpan = 1,
                            },
                        ],
                    },
                ],
            },
        ])
        {
            ContentData =
            [
                new(contentElementUdi, elementType.Key, elementType.Alias)
                {
                    RawPropertyValues = new()
                    {
                        { "singleLineText", "The single line of text in the grid root" },
                        { "bodyText", "<p>The body text in the grid root</p>" },
                    },
                },
                new(contentAreaElementUdi, elementType.Key, elementType.Alias)
                {
                    RawPropertyValues = new()
                    {
                        { "singleLineText", "The single line of text in the grid area" },
                        { "bodyText", "<p>The body text in the grid area</p>" },
                    },
                },
            ],
        };
        var propertyValue = JsonSerializer.Serialize(blockGridValue);

        var content = ContentBuilder.CreateBasicContent(contentType);
        content.Properties["blocks"]!.SetValue(propertyValue);
        ContentService.Save(content);

        var indexValues = editor.PropertyIndexValueFactory.GetIndexValues(
            content.Properties["blocks"]!,
            culture: null,
            segment: null,
            published: false,
            availableCultures: Enumerable.Empty<string>(),
            contentTypeDictionary: new Dictionary<Guid, IContentType>
            {
                { elementType.Key, elementType }, { contentType.Key, contentType }
            }).ToDictionary();

        Assert.IsTrue(indexValues.TryGetValue("blocks", out var blocksIndexValues));

        Assert.AreEqual(1, blocksIndexValues.Count());
        var blockIndexValue = blocksIndexValues.First() as string;
        Assert.IsNotNull(blockIndexValue);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(blockIndexValue.Contains("The single line of text in the grid root"));
            Assert.IsTrue(blockIndexValue.Contains("The body text in the grid root"));
            Assert.IsTrue(blockIndexValue.Contains("The single line of text in the grid area"));
            Assert.IsTrue(blockIndexValue.Contains("The body text in the grid area"));
        });
    }
}
