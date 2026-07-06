using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Search.Integration.Services;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Tests.Search.Integration.Tests;

// NOTE:
// Block editors share both the property value format ("ContentData") and the property value handling for search
// indexing. Therefore, the Block Grid property value handler tests are quite sparse and merely exist to prove
// basic indexing works.
// The Block List property value handler tests comprise a more exhaustive set of tests to prove the more intricate
// parts of block property value handling.
public class BlockGridPropertyValueHandlerTests : PropertyValueHandlerTestsBase
{
    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    [SetUp]
    public async Task SetUp()
    {
        await GetRequiredService<ILanguageService>()
            .CreateAsync(new Language("da-DK", "Danish (Denmark)"), Constants.Security.SuperUserKey);

        await GetRequiredService<ILanguageService>()
            .CreateAsync(new Language("de-DE", "German (Germany)"), Constants.Security.SuperUserKey);

        IndexerAndSearcher.Reset();
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);

        // this is necessary to test publishing of invariant block level properties without publishing the default language
        services.Configure<ContentSettings>(options => options.AllowEditInvariantFromNonDefault = true);
    }

    [Test]
    public async Task AllSimpleEditors_CanBeIndexed()
    {
        var (contentType, elementType) = await SetupSimpleEditorsTest();

        var contentElementKey = Guid.NewGuid();
        var blockListValue = new BlockGridValue([
            new () { ContentKey = contentElementKey }
        ])
        {
            ContentData =
            [
                new (contentElementKey, elementType.Key, elementType.Alias)
                {
                    Values =
                    [
                        new ()
                        {
                            Alias = "textBoxValue",
                            Value = "The TextBox value"
                        },
                        new ()
                        {
                            Alias = "textAreaValue",
                            Value = "The TextArea value"
                        },
                        new ()
                        {
                            Alias = "integerValue",
                            Value = 1234
                        },
                        new ()
                        {
                            Alias = "decimalValue",
                            Value = 56.78m
                        },
                        new ()
                        {
                            Alias = "dateValue",
                            Value = new DateTime(2001, 02, 03)
                        },
                        new ()
                        {
                            Alias = "dateAndTimeValue",
                            Value = new DateTime(2004, 05, 06, 07, 08, 09)
                        },
                        new ()
                        {
                            Alias = "tagsAsJsonValue",
                            Value = "[\"One\",\"Two\",\"Three\"]"
                        },
                        new ()
                        {
                            Alias = "tagsAsCsvValue",
                            Value = "Four,Five,Six"
                        },
                        new ()
                        {
                            Alias = "multipleTextstringsValue",
                            Value = "First\nSecond\nThird"
                        },
                        new ()
                        {
                            Alias = "contentPickerValue",
                            Value = "udi://document/55bf7f6d-acd2-4f1e-92bd-f0b5c41dbfed"
                        },
                        new ()
                        {
                            Alias = "booleanAsBooleanValue",
                            Value = true
                        },
                        new ()
                        {
                            Alias = "booleanAsIntegerValue",
                            Value = 1
                        },
                        new ()
                        {
                            Alias = "booleanAsStringValue",
                            Value = "1"
                        },
                        new ()
                        {
                            Alias = "sliderSingleValue",
                            Value = "1.23"
                        },
                        new ()
                        {
                            Alias = "sliderRangeValue",
                            Value = "2.34,5.67"
                        },
                        new ()
                        {
                            Alias = "multiUrlPickerValue",
                            Value = JsonSerializer.Serialize(new []
                            {
                                new MultiUrlPickerValueEditor.LinkDto
                                {
                                    Name = "Link One"
                                }
                            })
                        },
                    ]
                }
            ],
            Expose =
            [
                new BlockItemVariation(contentElementKey, null, null)
            ]
        };
        var blocksPropertyValue = JsonSerializer.Serialize(blockListValue);

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { blocks = blocksPropertyValue })
            .Build();
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        AssertDocumentFields(IndexAliases.DraftContent);
        AssertDocumentFields(IndexAliases.PublishedContent);

        TestIndexDocument document = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? tagsValue = document.Fields.FirstOrDefault(f => f.FieldName == Cms.Search.Core.Constants.FieldNames.Tags)?.Value;
        Assert.That(tagsValue, Is.Not.Null);
        CollectionAssert.AreEquivalent(new [] { "One", "Two", "Three", "Four", "Five", "Six" }, tagsValue.Keywords);

        return;

        void AssertDocumentFields(string indexAlias)
        {
            IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(indexAlias);
            Assert.That(documents, Has.Count.EqualTo(1));

            TestIndexDocument document = documents.Single();

            IndexValue? indexValue = document.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
            Assert.That(indexValue, Is.Not.Null);

            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(new[] { "The TextBox value", "The TextArea value", "First", "Second", "Third", "Link One" }, indexValue.Texts);

                CollectionAssert.AreEqual(new [] { 1234, 1, 1, 1 }, indexValue.Integers);

                CollectionAssert.AreEqual(new [] { 56.78m, 1.23m, 2.34m, 5.67m }, indexValue.Decimals);

                CollectionAssert.AreEqual(
                    new[]
                    {
                        new DateTimeOffset(new DateOnly(2001, 02, 03), new TimeOnly(), TimeSpan.Zero),
                        new DateTimeOffset(new DateOnly(2004, 05, 06), new TimeOnly(07, 08, 09), TimeSpan.Zero)
                    },
                    indexValue.DateTimeOffsets);

                CollectionAssert.AreEqual(new [] { "One", "Two", "Three", "Four", "Five", "Six", "55bf7f6d-acd2-4f1e-92bd-f0b5c41dbfed" }, indexValue.Keywords);
            });
        }
    }

    [Test]
    public async Task NestedBlockEditors_CanBeIndexed()
    {
        var (contentType, rootElementType, nestedElementType) = await SetupNestedBlockEditorsTest();

        var rootElement1Key = Guid.NewGuid();
        var rootElement2Key = Guid.NewGuid();
        var nestedElement1Key = Guid.NewGuid();
        var blockGridValue = new BlockGridValue([
            new () { ContentKey = rootElement1Key },
            new () { ContentKey = rootElement2Key }
        ])
        {
            ContentData =
            [
                new (rootElement1Key, rootElementType.Key, rootElementType.Alias)
                {
                    Values =
                    [
                        new ()
                        {
                            Alias = "nestedBlocks",
                            Value = JsonSerializer.Serialize(
                                new BlockGridValue([
                                    new () { ContentKey = nestedElement1Key }
                                ])
                                {
                                    ContentData =
                                    [
                                        new (nestedElement1Key, nestedElementType.Key, nestedElementType.Alias)
                                        {
                                            Values = [
                                                new ()
                                                {
                                                    Alias = "textBoxValue",
                                                    Value = "Nested TextBox value"
                                                },
                                                new ()
                                                {
                                                    Alias = "integerValue",
                                                    Value = 22
                                                },
                                                new ()
                                                {
                                                    Alias = "tagsAsJsonValue",
                                                    Value = "[\"One\",\"Two\",\"Three\"]"
                                                },
                                            ]
                                        }
                                    ],
                                    Expose =
                                    [
                                        new BlockItemVariation(nestedElement1Key, null, null)
                                    ]
                                })
                        }
                    ]
                },
                new (rootElement2Key, nestedElementType.Key, nestedElementType.Alias)
                {
                    Values =
                    [
                        new ()
                        {
                            Alias = "textBoxValue",
                            Value = "Root TextBox value"
                        },
                        new ()
                        {
                            Alias = "integerValue",
                            Value = 12
                        },
                        new ()
                        {
                            Alias = "tagsAsJsonValue",
                            Value = "[\"Four\",\"Five\",\"Six\"]"
                        }
                    ]
                }
            ],
            Expose =
            [
                new BlockItemVariation(rootElement1Key, null, null),
                new BlockItemVariation(rootElement2Key, null, null),
            ]
        };
        var blocksPropertyValue = JsonSerializer.Serialize(blockGridValue);

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { rootBlocks = blocksPropertyValue })
            .Build();
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument document = documents.Single();

        IndexValue? indexValue = document.Fields.FirstOrDefault(f => f.FieldName == "rootBlocks")?.Value;
        Assert.That(indexValue, Is.Not.Null);

        Assert.Multiple(() =>
        {
            CollectionAssert.AreEqual(new[] { "Nested TextBox value", "Root TextBox value" }, indexValue.Texts);
            CollectionAssert.AreEqual(new[] { 22, 12 }, indexValue.Integers);
            CollectionAssert.AreEqual(new[] { "One", "Two", "Three", "Four", "Five", "Six" }, indexValue.Keywords);
        });

        IndexValue? tagsValue = document.Fields.FirstOrDefault(f => f.FieldName == Cms.Search.Core.Constants.FieldNames.Tags)?.Value;
        Assert.That(tagsValue, Is.Not.Null);
        CollectionAssert.AreEquivalent(new [] { "One", "Two", "Three", "Four", "Five", "Six" }, tagsValue.Keywords);
    }

    private async Task<IContentType> CreateAllSimpleEditorsElementType(bool forBlockLevelVariance = false)
    {
        IContentType elementType = await CreateAllSimpleEditorsContentType();
        elementType.IsElement = true;

        if (forBlockLevelVariance)
        {
            elementType.Variations = ContentVariation.Culture;
            elementType.PropertyTypes.First(p => p.Alias == "textBoxValue").Variations = ContentVariation.Culture;
            elementType.PropertyTypes.First(p => p.Alias == "integerValue").Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(elementType, Constants.Security.SuperUserKey);
        return elementType;
    }

    private async Task<(IContentType ContentType, IContentType ElementType)> SetupSimpleEditorsTest(bool forBlockLevelVariance = false)
    {
        IContentType elementType = await CreateAllSimpleEditorsElementType(forBlockLevelVariance);

        var blockGridDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockGrid], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockGridConfiguration.BlockGridBlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementType.Key }
                    }
                }
            },
            Name = "My Block Grid",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };

        await GetRequiredService<IDataTypeService>().CreateAsync(blockGridDataType, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("blockEditor")
            .WithName("Block Editor")
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("blocks")
            .WithDataTypeId(blockGridDataType.Id)
            .Done()
            .Build();

        if (forBlockLevelVariance)
        {
            contentType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        return (contentType, elementType);
    }

    private async Task<(IContentType ContentType, IContentType RootElementType, IContentType NestedElementType)> SetupNestedBlockEditorsTest(bool forBlockLevelVariance = false)
    {
        IDataTypeService dataTypeService = GetRequiredService<IDataTypeService>();

        IContentType nestedElementType = await CreateAllSimpleEditorsElementType(forBlockLevelVariance);

        var nestedBlockGridDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockGrid], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockGridConfiguration.BlockGridBlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = nestedElementType.Key }
                    }
                }
            },
            Name = "My Nested Block Grid",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await dataTypeService.CreateAsync(nestedBlockGridDataType, Constants.Security.SuperUserKey);

        IContentType rootElementType = new ContentTypeBuilder()
            .WithAlias("rootBlockEditor")
            .WithName("Block Editor")
            .WithIsElement(true)
            .AddPropertyType()
            .WithAlias("nestedBlocks")
            .WithName("Nested Blocks")
            .WithDataTypeId(nestedBlockGridDataType.Id)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(rootElementType, Constants.Security.SuperUserKey);

        var rootBlockGridDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockGrid], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockGridConfiguration.BlockGridBlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = rootElementType.Key },
                        new() { ContentElementTypeKey = nestedElementType.Key }
                    }
                }
            },
            Name = "My Root Block Grid",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await dataTypeService.CreateAsync(rootBlockGridDataType, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("blockEditor")
            .WithName("Block Editor")
            .WithContentVariation(forBlockLevelVariance ? ContentVariation.Culture : ContentVariation.Nothing)
            .AddPropertyType()
            .WithAlias("rootBlocks")
            .WithName("Root Blocks")
            .WithDataTypeId(rootBlockGridDataType.Id)
            .Done()
            .Build();
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        return (contentType, rootElementType, nestedElementType);
    }
}
