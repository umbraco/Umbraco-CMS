using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

internal partial class BlockListElementLevelVariationTests
{
    private IContentValidationService ContentValidationService => GetRequiredService<IContentValidationService>();

    [Test]
    public async Task Can_Validate_Invalid_Properties()
    {
        var elementType = CreateElementTypeWithValidation();
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);
        var blockListValue = BlockListPropertyValue(
            elementType,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BlockProperty(
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "Invalid invariant content value" },
                    new() { Alias = "variantText", Value = "Valid content value in English", Culture = "en-US" },
                    new() { Alias = "variantText", Value = "Invalid content value in Danish", Culture = "da-DK" },
                },
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "Valid invariant settings value" },
                    new() { Alias = "variantText", Value = "Invalid settings value in English", Culture = "en-US" },
                    new() { Alias = "variantText", Value = "Valid settings value in Danish", Culture = "da-DK" },
                },
                null,
                null));

        var result = await ContentValidationService.ValidatePropertiesAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Variants =
                [
                    new VariantModel { Name = "Name en-US", Culture = "en-US", Segment = null },
                    new VariantModel { Name = "Name da-DK", Culture = "da-DK", Segment = null }
                ],
                Properties =
                [
                    new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
                ]
            },
            contentType);

        var errors = result.ValidationErrors.ToArray();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, errors.Length);
            Assert.IsTrue(errors.All(error => error.Alias == "blocks" && error.Culture == null && error.Segment == null));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[0].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[2].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".settingsData[0].values[1].value"));
        });
    }

    [Test]
    public async Task Can_Validate_Invalid_Properties_Nested_Blocks()
    {
        var (rootElementType, nestedElementType) = await CreateElementTypeWithValidationAndNestedBlocksAsync();
        var rootBlockListDataType = await CreateBlockListDataType(rootElementType);
        var contentType = CreateContentType(ContentVariation.Culture, rootBlockListDataType);

        var blockListValue = BlockListPropertyValue(
            rootElementType,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BlockProperty(
                new List<BlockPropertyValue>
                {
                    new()
                    {
                        Alias = "nestedBlocks",
                        Value = BlockListPropertyValue(
                            nestedElementType,
                            Guid.NewGuid(),
                            Guid.NewGuid(),
                            new BlockProperty(
                                new List<BlockPropertyValue>
                                {
                                    new() { Alias = "invariantText", Value = "Invalid nested invariant content value" },
                                    new() { Alias = "variantText", Value = "Valid nested content value in English", Culture = "en-US" },
                                    new() { Alias = "variantText", Value = "Invalid nested content value in Danish", Culture = "da-DK" },
                                },
                                new List<BlockPropertyValue>
                                {
                                    new() { Alias = "invariantText", Value = "Valid nested invariant settings value" },
                                    new() { Alias = "variantText", Value = "Invalid nested settings value in English", Culture = "en-US" },
                                    new() { Alias = "variantText", Value = "Valid nested settings value in Danish", Culture = "da-DK" },
                                },
                                null,
                                null))
                    },
                    new() { Alias = "invariantText", Value = "Invalid invariant content value" },
                    new() { Alias = "variantText", Value = "Valid content value in English", Culture = "en-US" },
                    new() { Alias = "variantText", Value = "Invalid content value in Danish", Culture = "da-DK" },
                },
                new List<BlockPropertyValue>
                {
                    new()
                    {
                        Alias = "nestedBlocks",
                        Value = BlockListPropertyValue(
                            nestedElementType,
                            Guid.NewGuid(),
                            Guid.NewGuid(),
                            new BlockProperty(
                                new List<BlockPropertyValue>
                                {
                                    new() { Alias = "invariantText", Value = "Valid nested invariant content value" },
                                    new() { Alias = "variantText", Value = "Invalid nested content value in English", Culture = "en-US" },
                                    new() { Alias = "variantText", Value = "Valid nested content value in Danish", Culture = "da-DK" },
                                },
                                new List<BlockPropertyValue>
                                {
                                    new() { Alias = "invariantText", Value = "Invalid nested invariant settings value" },
                                    new() { Alias = "variantText", Value = "Valid nested settings value in English", Culture = "en-US" },
                                    new() { Alias = "variantText", Value = "Invalid nested settings value in Danish", Culture = "da-DK" },
                                },
                                null,
                                null))
                    },
                    new() { Alias = "invariantText", Value = "Valid invariant content value" },
                    new() { Alias = "variantText", Value = "Invalid content value in English", Culture = "en-US" },
                    new() { Alias = "variantText", Value = "Valid content value in Danish", Culture = "da-DK" },
                },
                null,
                null));

        var result = await ContentValidationService.ValidatePropertiesAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Variants =
                [
                    new VariantModel { Name = "Name en-US", Culture = "en-US", Segment = null },
                    new VariantModel { Name = "Name da-DK", Culture = "da-DK", Segment = null }
                ],
                Properties =
                [
                    new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
                ]
            },
            contentType);

        var errors = result.ValidationErrors.ToArray();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(9, errors.Length);
            Assert.IsTrue(errors.All(error => error.Alias == "blocks" && error.Culture == null && error.Segment == null));

            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[0].value.contentData[0].values[0].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[0].value.contentData[0].values[2].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[0].value.settingsData[0].values[1].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[1].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[3].value"));

            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".settingsData[0].values[0].value.contentData[0].values[1].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".settingsData[0].values[0].value.settingsData[0].values[0].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".settingsData[0].values[0].value.settingsData[0].values[2].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".settingsData[0].values[2].value"));
        });
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureAllowEditInvariantFromNonDefaultTrue))]
    public async Task Can_Validate_Invalid_Properties_Specific_Culture_Only_With_AllowEditInvariantFromNonDefault()
        => await Can_Validate_Invalid_Properties_Specific_Culture_Only();

    [Test]
    public async Task Can_Validate_Invalid_Properties_Specific_Culture_Only_Without_AllowEditInvariantFromNonDefault()
        => await Can_Validate_Invalid_Properties_Specific_Culture_Only();

    private async Task Can_Validate_Invalid_Properties_Specific_Culture_Only()
    {
        var elementType = CreateElementTypeWithValidation();
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);
        var blockListValue = BlockListPropertyValue(
            elementType,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BlockProperty(
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "Invalid invariant content value" },
                    new() { Alias = "variantText", Value = "Valid content value in English", Culture = "en-US" },
                    new() { Alias = "variantText", Value = "Invalid content value in Danish", Culture = "da-DK" },
                },
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "Valid invariant settings value" },
                    new() { Alias = "variantText", Value = "Invalid settings value in English", Culture = "en-US" },
                    new() { Alias = "variantText", Value = "Valid settings value in Danish", Culture = "da-DK" },
                },
                null,
                null));

        var result = await ContentValidationService.ValidatePropertiesAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Variants =
                [
                    new VariantModel { Name = "Name en-US", Culture = "en-US", Segment = null },
                    new VariantModel { Name = "Name da-DK", Culture = "da-DK", Segment = null }
                ],
                Properties =
                [
                    new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
                ]
            },
            contentType,
            new[] { "en-US" });

        // NOTE: since the default culture is being validated, we expect the same result regardless
        //       of the AllowEditInvariantFromNonDefault configuration
        var errors = result.ValidationErrors.ToArray();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, errors.Length);
            Assert.IsTrue(errors.All(error => error.Alias == "blocks" && error.Culture == null && error.Segment == null));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[0].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".settingsData[0].values[1].value"));
        });
    }

    [Test]
    public async Task Can_Validate_Invalid_Properties_With_Wildcard_Culture()
    {
        var elementType = CreateElementTypeWithValidation();
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);
        var blockListValue = BlockListPropertyValue(
            elementType,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BlockProperty(
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "Invalid invariant content value" },
                    new() { Alias = "variantText", Value = "Valid content value in English", Culture = "en-US" },
                    new() { Alias = "variantText", Value = "Invalid content value in Danish", Culture = "da-DK" },
                },
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "Valid invariant settings value" },
                    new() { Alias = "variantText", Value = "Invalid settings value in English", Culture = "en-US" },
                    new() { Alias = "variantText", Value = "Valid settings value in Danish", Culture = "da-DK" },
                },
                null,
                null));

        var result = await ContentValidationService.ValidatePropertiesAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Variants =
                [
                    new VariantModel { Name = "Name en-US", Culture = "en-US", Segment = null },
                    new VariantModel { Name = "Name da-DK", Culture = "da-DK", Segment = null }
                ],
                Properties =
                [
                    new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
                ]
            },
            contentType,
            ["*"]);

        var errors = result.ValidationErrors.ToArray();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, errors.Length);
            Assert.IsTrue(errors.All(error => error.Alias == "blocks" && error.Culture == null && error.Segment == null));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[0].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[2].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".settingsData[0].values[1].value"));
        });
    }

    [Test]
    public async Task Can_Validate_Missing_Properties()
    {
        var elementType = CreateElementTypeWithValidation();
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);
        var blockListValue = BlockListPropertyValue(
            elementType,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BlockProperty(
                new List<BlockPropertyValue>
                {
                    // missing the mandatory "invariantText" (invariant) and "variantText" (in Danish)
                    new() { Alias = "variantText", Value = "Valid content value in English", Culture = "en-US" },
                },
                new List<BlockPropertyValue>
                {
                    // missing the mandatory "variantText" (in English)
                    new() { Alias = "invariantText", Value = "Valid invariant settings value" },
                    new() { Alias = "variantText", Value = "Valid settings value in Danish", Culture = "da-DK" },
                },
                null,
                null));

        // make sure all blocks are exposed
        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "en-US" },
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "da-DK" },
        ];

        var result = await ContentValidationService.ValidatePropertiesAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Variants =
                [
                    new VariantModel { Name = "Name en-US", Culture = "en-US", Segment = null },
                    new VariantModel { Name = "Name da-DK", Culture = "da-DK", Segment = null }
                ],
                Properties =
                [
                    new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
                ]
            },
            contentType);

        var errors = result.ValidationErrors.ToArray();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, errors.Length);
            Assert.IsTrue(errors.All(error => error.Alias == "blocks" && error.Culture == null && error.Segment == null));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[?(@.alias == 'invariantText' && @.culture == null && @.segment == null)].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[?(@.alias == 'variantText' && @.culture == 'da-DK' && @.segment == null)].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".settingsData[0].values[?(@.alias == 'variantText' && @.culture == 'en-US' && @.segment == null)].value"));
        });
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureAllowEditInvariantFromNonDefaultTrue))]
    public async Task Can_Validate_Missing_Properties_Nested_Blocks_Specific_Culture_Only_With_AllowEditInvariantFromNonDefault()
        => Can_Validate_Missing_Properties_Nested_Blocks_Specific_Culture_Only(true);

    [Test]
    public async Task Can_Validate_Missing_Properties_Nested_Blocks_Specific_Culture_Only_Without_AllowEditInvariantFromNonDefault()
        => Can_Validate_Missing_Properties_Nested_Blocks_Specific_Culture_Only(false);

    private async Task Can_Validate_Missing_Properties_Nested_Blocks_Specific_Culture_Only(bool expectedInvariantValidationErrors)
    {
        var (rootElementType, nestedElementType) = await CreateElementTypeWithValidationAndNestedBlocksAsync();
        var rootBlockListDataType = await CreateBlockListDataType(rootElementType);
        var contentType = CreateContentType(ContentVariation.Culture, rootBlockListDataType);

        var nestedContentBlocks = BlockListPropertyValue(
            nestedElementType,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BlockProperty(
                new List<BlockPropertyValue>
                {
                    // missing the mandatory "invariantText" (invariant) and "variantText" (in Danish)
                    new() { Alias = "variantText", Value = "Valid nested content value in English", Culture = "en-US" },
                },
                new List<BlockPropertyValue>
                {
                    // missing the mandatory "variantText" (in English)
                    new() { Alias = "invariantText", Value = "Valid nested invariant settings value" },
                    new() { Alias = "variantText", Value = "Valid nested settings value in Danish", Culture = "da-DK" },
                },
                null,
                null));

        var nestedSettingsBlocks = BlockListPropertyValue(
            nestedElementType,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BlockProperty(
                new List<BlockPropertyValue>
                {
                    // missing the mandatory "variantText" (in English)
                    new() { Alias = "invariantText", Value = "Valid nested invariant content value" },
                    new() { Alias = "variantText", Value = "Valid nested content value in Danish", Culture = "da-DK" },
                },
                new List<BlockPropertyValue>
                {
                    // missing the mandatory "invariantText" (invariant) and "variantText" (in Danish)
                    new() { Alias = "variantText", Value = "Valid nested settings value in English", Culture = "en-US" },
                },
                null,
                null));

        // make sure all nested blocks are exposed
        nestedContentBlocks.Expose =
        [
            new() { ContentKey = nestedContentBlocks.ContentData[0].Key, Culture = "en-US" },
            new() { ContentKey = nestedContentBlocks.ContentData[0].Key, Culture = "da-DK" },
        ];
        nestedSettingsBlocks.Expose =
        [
            new() { ContentKey = nestedSettingsBlocks.ContentData[0].Key, Culture = "en-US" },
            new() { ContentKey = nestedSettingsBlocks.ContentData[0].Key, Culture = "da-DK" },
        ];

        var blockListValue = BlockListPropertyValue(
            rootElementType,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BlockProperty(
                new List<BlockPropertyValue>
                {
                    new()
                    {
                        Alias = "nestedBlocks",
                        Value = nestedContentBlocks
                    },
                    // missing the mandatory "variantText" (in both English and Danish)
                    new() { Alias = "invariantText", Value = "Valid root invariant content value" }
                },
                new List<BlockPropertyValue>
                {
                    new()
                    {
                        Alias = "nestedBlocks",
                        Value = nestedSettingsBlocks
                    },
                    // missing the mandatory "invariantText"
                    new() { Alias = "variantText", Value = "Valid root settings value in English", Culture = "en-US" },
                    new() { Alias = "variantText", Value = "Valid root settings value in Danish", Culture = "da-DK" }
                },
                null,
                null));

        // make sure all root blocks are exposed
        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "en-US" },
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "da-DK" },
        ];

        var result = await ContentValidationService.ValidatePropertiesAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Variants =
                [
                    new VariantModel { Name = "Name en-US", Culture = "en-US", Segment = null },
                    new VariantModel { Name = "Name da-DK", Culture = "da-DK", Segment = null }
                ],
                Properties =
                [
                    new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
                ]
            },
            contentType,
            new[] { "da-DK" });

        var errors = result.ValidationErrors.ToArray();

        // NOTE: since the default culture is not being validated, we expect different results depending
        //       on the AllowEditInvariantFromNonDefault configuration

        if (expectedInvariantValidationErrors)
        {
            Assert.Multiple(() =>
            {
                Assert.AreEqual(6, errors.Length);
                Assert.IsTrue(errors.All(error => error.Alias == "blocks" && error.Culture == null && error.Segment == null));

                Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[0].value.contentData[0].values[?(@.alias == 'invariantText' && @.culture == null && @.segment == null)].value"));
                Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[0].value.contentData[0].values[?(@.alias == 'variantText' && @.culture == 'da-DK' && @.segment == null)].value"));
                Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[?(@.alias == 'variantText' && @.culture == 'da-DK' && @.segment == null)].value"));

                Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".settingsData[0].values[0].value.settingsData[0].values[?(@.alias == 'invariantText' && @.culture == null && @.segment == null)].value"));
                Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".settingsData[0].values[0].value.settingsData[0].values[?(@.alias == 'variantText' && @.culture == 'da-DK' && @.segment == null)].value"));
                Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".settingsData[0].values[?(@.alias == 'invariantText' && @.culture == null && @.segment == null)].value"));
            });
        }
        else
        {
            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, errors.Length);
                Assert.IsTrue(errors.All(error => error.Alias == "blocks" && error.Culture == null && error.Segment == null));

                Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[0].value.contentData[0].values[?(@.alias == 'variantText' && @.culture == 'da-DK' && @.segment == null)].value"));
                Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[?(@.alias == 'variantText' && @.culture == 'da-DK' && @.segment == null)].value"));

                Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".settingsData[0].values[0].value.settingsData[0].values[?(@.alias == 'variantText' && @.culture == 'da-DK' && @.segment == null)].value"));
            });
        }
    }

    [Test]
    public async Task Does_Not_Validate_Unexposed_Blocks()
    {
        var elementType = CreateElementTypeWithValidation();
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);
        var blockListValue = BlockListPropertyValue(
            elementType,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BlockProperty(
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "Valid invariant content value" },
                    new() { Alias = "variantText", Value = "Valid content value in English", Culture = "en-US" },
                },
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "Valid invariant settings value" },
                    new() { Alias = "variantText", Value = "Valid settings value in English", Culture = "en-US" },
                },
                null,
                null));

        // only expose the block in English
        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "en-US" },
        ];

        var result = await ContentValidationService.ValidatePropertiesAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Variants =
                [
                    new VariantModel { Name = "Name en-US", Culture = "en-US", Segment = null },
                    new VariantModel { Name = "Name da-DK", Culture = "da-DK", Segment = null }
                ],
                Properties =
                [
                    new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
                ]
            },
            contentType,
            ["da-DK"]);

        Assert.IsEmpty(result.ValidationErrors);
    }

    [Test]
    public async Task Can_Validate_Properties_Variant_Blocks()
    {
        var elementType = CreateElementTypeWithValidation(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType, ContentVariation.Culture);
        var blockListValue = BlockListPropertyValue(
            elementType,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BlockProperty(
                new List<BlockPropertyValue>
                {
                    // blocks property values use null culture for culture variant block editor properties
                    new() { Alias = "invariantText", Value = "Valid invariantText content value", Culture = null },
                    new() { Alias = "variantText", Value = "Invalid variantText content value", Culture = null },
                },
                new List<BlockPropertyValue>
                {
                    // blocks property values use null culture for culture variant block editor properties
                    new() { Alias = "invariantText", Value = "Invalid invariantText settings value", Culture = null },
                    new() { Alias = "variantText", Value = "Valid variantText settings value", Culture = null },
                },
                "en-US",
                null));

        // make sure all blocks are exposed as they would be for culture variant properties
        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = null }
        ];

        var result = await ContentValidationService.ValidatePropertiesAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Variants =
                [
                    new VariantModel { Name = "Name en-US", Culture = "en-US", Segment = null }
                ],
                Properties =
                [
                    new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue), Culture = "en-US" }
                ]
            },
            contentType);

        var errors = result.ValidationErrors.ToArray();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, errors.Length);
            Assert.IsTrue(errors.All(error => error.Alias == "blocks" && error.Culture == "en-US" && error.Segment == null));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[1].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".settingsData[0].values[0].value"));
        });
    }

    [Test]
    public async Task Can_Validate_Missing_Properties_Variant_Blocks()
    {
        var elementType = CreateElementTypeWithValidation(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType, ContentVariation.Culture);
        var blockListValue = BlockListPropertyValue(
            elementType,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BlockProperty(
                new List<BlockPropertyValue>
                {
                    // missing the mandatory "invariantText"
                    new() { Alias = "variantText", Value = "Valid variantText content value", Culture = null },
                },
                new List<BlockPropertyValue>
                {
                    // missing the mandatory "variantText" (which, to add to the confusion, is invariant at block level in this test case)
                    new() { Alias = "invariantText", Value = "Valid invariantText settings value", Culture = null },
                },
                "en-US",
                null));

        // make sure all blocks are exposed as they would be for culture variant properties
        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = null }
        ];

        var result = await ContentValidationService.ValidatePropertiesAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Variants =
                [
                    new VariantModel { Name = "Name en-US", Culture = "en-US", Segment = null }
                ],
                Properties =
                [
                    new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue), Culture = "en-US" }
                ]
            },
            contentType);

        var errors = result.ValidationErrors.ToArray();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, errors.Length);
            Assert.IsTrue(errors.All(error => error.Alias == "blocks" && error.Culture == "en-US" && error.Segment == null));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".contentData[0].values[?(@.alias == 'invariantText' && @.culture == null && @.segment == null)].value"));
            Assert.IsNotNull(errors.FirstOrDefault(error => error.JsonPath == ".settingsData[0].values[?(@.alias == 'variantText' && @.culture == null && @.segment == null)].value"));
        });
    }
}
