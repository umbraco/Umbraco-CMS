using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.SearchService;

public class VariantBlockTests : SearcherTestBase
{
    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    [TestCase("en-US", "Singleline")]
    [TestCase("da-DK", "Enkeltlinje")]
    [TestCase("en-US", "richtext")]
    [TestCase("da-DK", "rigtext")]
    public async Task Can_Search_TextBox_In_Block(string culture, string query)
    {
        var indexAlias = GetIndexAlias(false);
        await CreateBlockContent();

        SearchResult results = await Searcher.SearchAsync(indexAlias, query, culture: culture);

        Assert.That(results.Total, Is.EqualTo(1));
    }

    [TestCase("da-DK", "Singleline")]
    [TestCase("en-US", "Enkeltlinje")]
    [TestCase("da-DK", "richtext")]
    [TestCase("en-US", "rigtext")]
    public async Task Can_Not_Search_TextBox_In_Block_With_Wrong_Culture(string culture, string query)
    {
        var indexAlias = GetIndexAlias(false);
        await CreateBlockContent();

        SearchResult results = await Searcher.SearchAsync(indexAlias, query, culture: culture);

        Assert.That(results.Total, Is.EqualTo(0));
    }

    [TestCase("en-US")]
    [TestCase("da-DK")]
    public async Task Can_Search_Invariant_TextArea_In_Block_Both_Cultures(string culture)
    {
        var indexAlias = GetIndexAlias(false);
        await CreateBlockContent();

        SearchResult results = await Searcher.SearchAsync(indexAlias, "textarea", culture: culture);

        Assert.That(results.Total, Is.EqualTo(1));
    }

    [TestCase("da-DK", 100)]
    [TestCase("en-US", 50)]
    public async Task Can_Search_Integer_In_Block(string culture, int value)
    {
        var indexAlias = GetIndexAlias(false);
        await CreateBlockContent();

        SearchResult results = await Searcher.SearchAsync(indexAlias, culture: culture, filters: [new IntegerRangeFilter("blocks", [new IntegerRangeFilterRange(value - 10, value + 10)], false)]);

        Assert.That(results.Total, Is.EqualTo(1));
    }

    private async Task CreateBlockContent()
    {
        await LanguageService.CreateAsync(new Language("da-DK", "Danish"), Constants.Security.SuperUserKey);
        ContentType elementType = ContentTypeBuilder.CreateAllTypesContentType("myElementType", "My Element Type");
        elementType.Variations = ContentVariation.Culture;
        elementType.PropertyTypes.First(p => p.Alias == "singleLineText").Variations = ContentVariation.Culture;
        elementType.PropertyTypes.First(p => p.Alias == "bodyText").Variations = ContentVariation.Culture;
        elementType.PropertyTypes.First(p => p.Alias == "number").Variations = ContentVariation.Culture;

        elementType.IsElement = true;
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        IContentType blockListContentType = await CreateBlockListContentType(elementType);

        var contentElementKey = Guid.NewGuid();
        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList, [new BlockListLayoutItem { ContentKey = contentElementKey }]
                },
            },
            ContentData =
            [
                new BlockItemData
                {
                    Key = contentElementKey,
                    ContentTypeAlias = elementType.Alias,
                    ContentTypeKey = elementType.Key,
                    Values =
                    [
                        new BlockPropertyValue { Alias = "singleLineText", Value = "Singleline", Culture = "en-US" },
                        new BlockPropertyValue { Alias = "singleLineText", Value = "Enkeltlinje", Culture = "da-DK" },
                        new BlockPropertyValue { Alias = "multilineText", Value = "something with textarea" },
                        new BlockPropertyValue { Alias = "number", Value = 50, Culture = "en-US" },
                        new BlockPropertyValue { Alias = "number", Value = 100, Culture = "da-DK" },
                        new BlockPropertyValue { Alias = "bodyText", Value = "something with richtext", Culture = "en-US" },
                        new BlockPropertyValue { Alias = "bodyText", Value = "noget med rigtext", Culture = "da-DK" },
                        new BlockPropertyValue { Alias = "dateTime", Value = CurrentDateTime },
                    ],
                }
            ],
            Expose =
            [
                new BlockItemVariation(contentElementKey, null, null)
            ],
        };
        var blocksPropertyValue = JsonSerializer.Serialize(blockListValue);

        Content content = new ContentBuilder()
            .WithContentType(blockListContentType)
            .WithCultureName("en-US", "My Blocks EN")
            .WithCultureName("da-DK", "My Blocks DA")
            .WithPropertyValues(new { blocks = blocksPropertyValue })
            .Build();

        var indexAlias = GetIndexAlias(false);
        await WaitForIndexing(indexAlias, () =>
        {
            ContentService.Save(content);
            return Task.CompletedTask;
        });
    }

    private async Task<IContentType> CreateBlockListContentType(IContentType elementType)
    {
        var blockListDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementType.Key },
                    }
                },
            },
            Name = "My Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = CurrentDateTime,
        };

        await DataTypeService.CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("myPage")
            .WithName("My Page")
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("Blocks")
            .WithDataTypeId(blockListDataType.Id)
            .Done()
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // re-fetch to wire up all key bindings (particularly to the datatype)
        return await ContentTypeService.GetAsync(contentType.Key) ?? null!;
    }
}
