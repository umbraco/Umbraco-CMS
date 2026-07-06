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

public class BlockListPropertyValueHandlerTests : PropertyValueHandlerTestsBase
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
        var blockListValue = new BlockListValue([
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
        var nestedElement2Key = Guid.NewGuid();
        var blockListValue = new BlockListValue([
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
                                new BlockListValue([
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
                                                    Value = "TextBox value #1"
                                                },
                                                new ()
                                                {
                                                    Alias = "integerValue",
                                                    Value = 12
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
                new (rootElement2Key, rootElementType.Key, rootElementType.Alias)
                {
                    Values =
                    [
                        new ()
                        {
                            Alias = "nestedBlocks",
                            Value = JsonSerializer.Serialize(
                                new BlockListValue([
                                    new () { ContentKey = nestedElement2Key }
                                ])
                                {
                                    ContentData =
                                    [
                                        new (nestedElement2Key, nestedElementType.Key, nestedElementType.Alias)
                                        {
                                            Values = [
                                                new ()
                                                {
                                                    Alias = "textBoxValue",
                                                    Value = "TextBox value #2"
                                                },
                                                new ()
                                                {
                                                    Alias = "integerValue",
                                                    Value = 34
                                                },
                                                new ()
                                                {
                                                    Alias = "tagsAsJsonValue",
                                                    Value = "[\"Four\",\"Five\",\"Six\"]"
                                                },
                                            ]
                                        }
                                    ],
                                    Expose =
                                    [
                                        new BlockItemVariation(nestedElement2Key, null, null)
                                    ]
                                })
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
        var blocksPropertyValue = JsonSerializer.Serialize(blockListValue);

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
            CollectionAssert.AreEqual(new[] { "TextBox value #1", "TextBox value #2" }, indexValue.Texts);
            CollectionAssert.AreEqual(new[] { 12, 34 }, indexValue.Integers);
            CollectionAssert.AreEqual(new[] { "One", "Two", "Three", "Four", "Five", "Six" }, indexValue.Keywords);
        });

        IndexValue? tagsValue = document.Fields.FirstOrDefault(f => f.FieldName == Cms.Search.Core.Constants.FieldNames.Tags)?.Value;
        Assert.That(tagsValue, Is.Not.Null);
        CollectionAssert.AreEquivalent(new [] { "One", "Two", "Three", "Four", "Five", "Six" }, tagsValue.Keywords);
    }

    [Test]
    public async Task BlockLevelVariation_CanBeIndexed_WithAllCulturesPublished()
    {
        await SetupBlockLevelVarianceTest(true);

        AssertDocumentFields(IndexAliases.DraftContent);
        AssertDocumentFields(IndexAliases.PublishedContent);

        return;

        void AssertDocumentFields(string indexAlias)
        {
            IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(indexAlias);
            Assert.That(documents, Has.Count.EqualTo(1));

            TestIndexDocument document = documents.Single();

            IndexValue? indexValueInv = document.Fields.SingleOrDefault(f => f is { FieldName: "rootBlocks", Culture: null })?.Value;
            Assert.That(indexValueInv, Is.Not.Null);
            CollectionAssert.AreEqual(new[] { "Nested TextArea value INV", "Root TextArea value INV" }, indexValueInv.Texts);

            IndexValue? indexValueEn = document.Fields.SingleOrDefault(f => f is { FieldName: "rootBlocks", Culture: "en-US" })?.Value;
            Assert.That(indexValueEn, Is.Not.Null);

            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(new[] { "Nested TextBox value EN", "Root TextBox value EN" }, indexValueEn.Texts);
                CollectionAssert.AreEqual(new[] { 21, 11 }, indexValueEn.Integers);
            });

            IndexValue? indexValueDa = document.Fields.SingleOrDefault(f => f is { FieldName: "rootBlocks", Culture: "da-DK" })?.Value;
            Assert.That(indexValueDa, Is.Not.Null);

            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(new[] { "Nested TextBox value DA", "Root TextBox value DA" }, indexValueDa.Texts);
                CollectionAssert.AreEqual(new[] { 22, 12 }, indexValueDa.Integers);
            });

            IndexValue? indexValueDe = document.Fields.SingleOrDefault(f => f is { FieldName: "rootBlocks", Culture: "de-DE" })?.Value;
            Assert.That(indexValueDe, Is.Not.Null);

            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(new[] { "Nested TextBox value DE", "Root TextBox value DE" }, indexValueDe.Texts);
                CollectionAssert.AreEqual(new[] { 23, 13 }, indexValueDe.Integers);
            });
        }
    }

    [Test]
    public async Task BlockLevelVariation_CanBeIndexed_WithSomeCulturesPublished()
    {
        await SetupBlockLevelVarianceTest(false);

        AssertDocumentFields(IndexAliases.DraftContent, true);
        AssertDocumentFields(IndexAliases.PublishedContent, false);

        return;

        void AssertDocumentFields(string indexAlias, bool expectEnValues)
        {
            IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(indexAlias);
            Assert.That(documents, Has.Count.EqualTo(1));

            TestIndexDocument document = documents.Single();

            IndexValue? indexValueInv = document.Fields.SingleOrDefault(f => f is { FieldName: "rootBlocks", Culture: null })?.Value;
            Assert.That(indexValueInv, Is.Not.Null);
            CollectionAssert.AreEqual(new[] { "Nested TextArea value INV", "Root TextArea value INV" }, indexValueInv.Texts);

            IndexValue? indexValueEn = document.Fields.SingleOrDefault(f => f is { FieldName: "rootBlocks", Culture: "en-US" })?.Value;

            if (expectEnValues)
            {
                Assert.That(indexValueEn, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    CollectionAssert.AreEqual(new[] { "Nested TextBox value EN", "Root TextBox value EN" }, indexValueEn.Texts);
                    CollectionAssert.AreEqual(new[] { 21, 11 }, indexValueEn.Integers);
                });
            }
            else
            {
                Assert.That(indexValueEn, Is.Null);
            }

            IndexValue? indexValueDa = document.Fields.SingleOrDefault(f => f is { FieldName: "rootBlocks", Culture: "da-DK" })?.Value;
            Assert.That(indexValueDa, Is.Not.Null);

            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(new[] { "Nested TextBox value DA", "Root TextBox value DA" }, indexValueDa.Texts);
                CollectionAssert.AreEqual(new[] { 22, 12 }, indexValueDa.Integers);
            });

            IndexValue? indexValueDe = document.Fields.SingleOrDefault(f => f is { FieldName: "rootBlocks", Culture: "de-DE" })?.Value;
            Assert.That(indexValueDe, Is.Not.Null);

            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(new[] { "Nested TextBox value DE", "Root TextBox value DE" }, indexValueDe.Texts);
                CollectionAssert.AreEqual(new[] { 23, 13 }, indexValueDe.Integers);
            });
        }
    }

    [Test]
    public async Task BlockLevelVariation_CanBeIndexed_WithSomeElementsExposed()
    {
        var (contentType, elementType) = await SetupSimpleEditorsTest(true);
        var contentElement1Key = Guid.NewGuid();
        var contentElement2Key = Guid.NewGuid();
        var contentElement3Key = Guid.NewGuid();

        var blockListValue = new BlockListValue([
            new () { ContentKey = contentElement1Key },
            new () { ContentKey = contentElement2Key },
            new () { ContentKey = contentElement3Key }
        ])
        {
            ContentData =
            [
                new (contentElement1Key, elementType.Key, elementType.Alias)
                {
                    Values = [
                        new () { Alias = "textBoxValue", Value = "TextBox EN #1", Culture = "en-US" },
                        new () { Alias = "textBoxValue", Value = "TextBox DA #1", Culture = "da-DK" }
                    ]
                },
                new (contentElement2Key, elementType.Key, elementType.Alias)
                {
                    Values = [
                        new () { Alias = "textBoxValue", Value = "TextBox EN #2", Culture = "en-US" },
                        new () { Alias = "textBoxValue", Value = "TextBox DA #2", Culture = "da-DK" }
                    ]
                },
                new (contentElement3Key, elementType.Key, elementType.Alias)
                {
                    Values = [
                        new () { Alias = "textBoxValue", Value = "TextBox EN #3", Culture = "en-US" },
                        new () { Alias = "textBoxValue", Value = "TextBox DA #3", Culture = "da-DK" }
                    ]
                }
            ],
            Expose =
            [
                new BlockItemVariation(contentElement1Key, "en-US", null),
                new BlockItemVariation(contentElement1Key, "da-DK", null),
                new BlockItemVariation(contentElement2Key, "en-US", null),
                new BlockItemVariation(contentElement3Key, "da-DK", null)
            ]
        };
        var blocksPropertyValue = JsonSerializer.Serialize(blockListValue);

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "My Blocks EN")
            .WithCultureName("da-DK", "My Blocks DA")
            .WithPropertyValues(new { blocks = blocksPropertyValue })
            .Build();
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        AssertDocumentFields(
            IndexAliases.DraftContent,
            ["TextBox EN #1", "TextBox EN #2", "TextBox EN #3"],
            ["TextBox DA #1", "TextBox DA #2", "TextBox DA #3"]);
        AssertDocumentFields(
            IndexAliases.PublishedContent,
            ["TextBox EN #1", "TextBox EN #2"],
            ["TextBox DA #1", "TextBox DA #3"]);

        return;

        void AssertDocumentFields(string indexAlias, IEnumerable<string> expectedTextBoxValuesEn, IEnumerable<string> expectedTextBoxValuesDa)
        {
            IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(indexAlias);
            Assert.That(documents, Has.Count.EqualTo(1));

            TestIndexDocument document = documents.Single();

            IndexValue? indexValueEn = document.Fields.FirstOrDefault(f => f is { FieldName: "blocks", Culture: "en-US" })?.Value;
            Assert.That(indexValueEn, Is.Not.Null);
            CollectionAssert.AreEqual(expectedTextBoxValuesEn, indexValueEn.Texts);

            IndexValue? indexValueDa = document.Fields.FirstOrDefault(f => f is { FieldName: "blocks", Culture: "da-DK" })?.Value;
            Assert.That(indexValueDa, Is.Not.Null);
            CollectionAssert.AreEqual(expectedTextBoxValuesDa, indexValueDa.Texts);
        }
    }

    [Test]
    public async Task BlockLevelVariation_SupportsMultipleTextRelevance()
    {
        var (contentType, elementType) = await SetupMultipleTextRelevanceTest();
        var contentElement1Key = Guid.NewGuid();
        var contentElement2Key = Guid.NewGuid();

        var blockListValue = new BlockListValue([
            new () { ContentKey = contentElement1Key },
            new () { ContentKey = contentElement2Key }
        ])
        {
            ContentData =
            [
                new (contentElement1Key, elementType.Key, elementType.Alias)
                {
                    Values = [
                        new ()
                        {
                            Alias = "markdownValue",
                            Value = """
                                    # H1 Heading #1

                                    ## H2 Heading #1

                                    ### H3 Heading #1

                                    Paragraph #1
                                    """
                        }
                    ]
                },
                new (contentElement2Key, elementType.Key, elementType.Alias)
                {
                    Values = [
                        new ()
                        {
                            Alias = "markdownValue",
                            Value = """
                                    # H1 Heading #2

                                    ## H2 Heading #2

                                    ### H3 Heading #2

                                    Paragraph #2
                                    """
                        }
                    ]
                }
            ],
            Expose =
            [
                new BlockItemVariation(contentElement1Key, null, null),
                new BlockItemVariation(contentElement2Key, null, null)
            ]
        };
        var blocksPropertyValue = JsonSerializer.Serialize(blockListValue);

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { blocks = blocksPropertyValue })
            .Build();
        ContentService.Save(content);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(1));

        IndexValue? blocksValue = documents[0].Fields.FirstOrDefault(f => f.FieldName is "blocks")?.Value;
        Assert.That(blocksValue, Is.Not.Null);

        CollectionAssert.AreEqual(new [] { "H1 Heading #1", "H1 Heading #2"}, blocksValue.TextsR1);
        CollectionAssert.AreEqual(new [] { "H2 Heading #1", "H2 Heading #2"}, blocksValue.TextsR2);
        CollectionAssert.AreEqual(new [] { "H3 Heading #1", "H3 Heading #2"}, blocksValue.TextsR3);
        CollectionAssert.AreEqual(new [] { "Paragraph #1", "Paragraph #2"}, blocksValue.Texts);
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

        var blockListDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementType.Key }
                    }
                }
            },
            Name = "My Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };

        await GetRequiredService<IDataTypeService>().CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("blockEditor")
            .WithName("Block Editor")
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("blocks")
            .WithDataTypeId(blockListDataType.Id)
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

        var nestedBlockListDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = nestedElementType.Key }
                    }
                }
            },
            Name = "My Nested Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await dataTypeService.CreateAsync(nestedBlockListDataType, Constants.Security.SuperUserKey);

        IContentType rootElementType = new ContentTypeBuilder()
            .WithAlias("rootBlockEditor")
            .WithName("Block Editor")
            .WithIsElement(true)
            .AddPropertyType()
            .WithAlias("nestedBlocks")
            .WithName("Nested Blocks")
            .WithDataTypeId(nestedBlockListDataType.Id)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(rootElementType, Constants.Security.SuperUserKey);

        var rootBlockListDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = rootElementType.Key },
                        new() { ContentElementTypeKey = nestedElementType.Key }
                    }
                }
            },
            Name = "My Root Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await dataTypeService.CreateAsync(rootBlockListDataType, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("blockEditor")
            .WithName("Block Editor")
            .WithContentVariation(forBlockLevelVariance ? ContentVariation.Culture : ContentVariation.Nothing)
            .AddPropertyType()
            .WithAlias("rootBlocks")
            .WithName("Root Blocks")
            .WithDataTypeId(rootBlockListDataType.Id)
            .Done()
            .Build();
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        return (contentType, rootElementType, nestedElementType);
    }

    private async Task<(IContentType ContentType, IContentType ElementType)> SetupMultipleTextRelevanceTest()
    {
        IDataTypeService dataTypeService = GetRequiredService<IDataTypeService>();

        DataType markdownDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Markdown")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.MarkdownEditor)
            .Done()
            .Build();

        await dataTypeService.CreateAsync(markdownDataType, Constants.Security.SuperUserKey);

        IContentType elementType = new ContentTypeBuilder()
            .WithAlias("blockEditorElement")
            .WithName("Block Editor Element")
            .WithIsElement(true)
            .AddPropertyType()
            .WithAlias("markdownValue")
            .WithName("Markdown")
            .WithDataTypeId(markdownDataType.Id)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var blockListDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementType.Key },
                        new() { ContentElementTypeKey = elementType.Key }
                    }
                }
            },
            Name = "My Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await dataTypeService.CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("blockEditor")
            .WithName("Block Editor")
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("Blocks")
            .WithDataTypeId(blockListDataType.Id)
            .Done()
            .Build();
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        return (contentType, elementType);
    }

    private async Task SetupBlockLevelVarianceTest(bool publishAllCultures)
    {
        var (contentType, rootElementType, nestedElementType) = await SetupNestedBlockEditorsTest(true);

        var rootElement1Key = Guid.NewGuid();
        var rootElement2Key = Guid.NewGuid();
        var nestedElement1Key = Guid.NewGuid();
        var blockListValue = new BlockListValue([
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
                                new BlockListValue([
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
                                                    Value = "Nested TextBox value EN",
                                                    Culture = "en-US"
                                                },
                                                new ()
                                                {
                                                    Alias = "textBoxValue",
                                                    Value = "Nested TextBox value DA",
                                                    Culture = "da-DK"
                                                },
                                                new ()
                                                {
                                                    Alias = "textBoxValue",
                                                    Value = "Nested TextBox value DE",
                                                    Culture = "de-DE"
                                                },
                                                new ()
                                                {
                                                    Alias = "textAreaValue",
                                                    Value = "Nested TextArea value INV"
                                                },
                                                new ()
                                                {
                                                    Alias = "integerValue",
                                                    Value = 21,
                                                    Culture = "en-US"
                                                },
                                                new ()
                                                {
                                                    Alias = "integerValue",
                                                    Value = 22,
                                                    Culture = "da-DK"
                                                },
                                                new ()
                                                {
                                                    Alias = "integerValue",
                                                    Value = 23,
                                                    Culture = "de-DE"
                                                },
                                            ]
                                        }
                                    ],
                                    Expose =
                                    [
                                        new BlockItemVariation(nestedElement1Key, "en-US", null),
                                        new BlockItemVariation(nestedElement1Key, "da-DK", null),
                                        new BlockItemVariation(nestedElement1Key, "de-DE", null),
                                    ]
                                })
                        }
                    ]
                },
                new (rootElement2Key, nestedElementType.Key, nestedElementType.Alias)
                {
                    Values = [
                        new ()
                        {
                            Alias = "textBoxValue",
                            Value = "Root TextBox value EN",
                            Culture = "en-US"
                        },
                        new ()
                        {
                            Alias = "textBoxValue",
                            Value = "Root TextBox value DA",
                            Culture = "da-DK"
                        },
                        new ()
                        {
                            Alias = "textBoxValue",
                            Value = "Root TextBox value DE",
                            Culture = "de-DE"
                        },
                        new ()
                        {
                            Alias = "textAreaValue",
                            Value = "Root TextArea value INV"
                        },
                        new ()
                        {
                            Alias = "integerValue",
                            Value = 11,
                            Culture = "en-US"
                        },
                        new ()
                        {
                            Alias = "integerValue",
                            Value = 12,
                            Culture = "da-DK"
                        },
                        new ()
                        {
                            Alias = "integerValue",
                            Value = 13,
                            Culture = "de-DE"
                        },
                    ]
                }
            ],
            Expose =
            [
                new BlockItemVariation(rootElement1Key, "en-US", null),
                new BlockItemVariation(rootElement1Key, "da-DK", null),
                new BlockItemVariation(rootElement1Key, "de-DE", null),
                new BlockItemVariation(rootElement2Key, "en-US", null),
                new BlockItemVariation(rootElement2Key, "da-DK", null),
                new BlockItemVariation(rootElement2Key, "de-DE", null),
            ]
        };
        var blocksPropertyValue = JsonSerializer.Serialize(blockListValue);

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "My Blocks EN")
            .WithCultureName("da-DK", "My Blocks DA")
            .WithCultureName("de-DE", "My Blocks DE")
            .WithPropertyValues(new { rootBlocks = blocksPropertyValue })
            .Build();
        ContentService.Save(content);
        if (publishAllCultures)
        {
            ContentService.Publish(content, ["en-US", "da-DK", "de-DE"]);
        }
        else
        {
            ContentService.Publish(content, ["da-DK", "de-DE"]);
        }
    }
}
