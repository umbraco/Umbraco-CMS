using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.SearchService;

public class VariantFilterTest : SearcherTestBase
{
    [TestCase("en-US", true, 0)]
    [TestCase("en-US", false, 1)]
    [TestCase("da-DK", true, 0)]
    [TestCase("da-DK", false, 1)]
    [TestCase("ja-JP", true, 0)]
    [TestCase("ja-JP", false, 1)]
    public async Task ByCulture_CanFilterByPathIds(string culture, bool negate, int expectedCount)
    {
        await CreateVariantDocument();

        var indexAlias = GetIndexAlias(false);

        SearchResult results = await Searcher.SearchAsync(
            indexAlias: indexAlias,
            filters: new List<Filter> { new KeywordFilter("Umb_Id", [RootKey.ToString()], negate) },
            culture: culture);

        Assert.That(results.Total, Is.EqualTo(expectedCount));
    }

    [TestCase("Root", "en-US", true, 0)]
    [TestCase("Root", "en-US", false, 1)]
    [TestCase("Rod", "da-DK", true, 0)]
    [TestCase("Rod", "da-DK", false, 1)]
    [TestCase("ル-ト", "ja-JP", true, 0)]
    [TestCase("ル-ト", "ja-JP", false, 1)]
    public async Task CanFilterByTextByCulture(string text, string culture, bool negate, int expectedCount)
    {
        await CreateVariantDocument();

        var indexAlias = GetIndexAlias(false);

        SearchResult results = await Searcher.SearchAsync(
            indexAlias: indexAlias,
            filters: new List<Filter> { new TextFilter("title", [text], negate) },
            culture: culture);

        Assert.That(results.Total, Is.EqualTo(expectedCount));
    }

    [TestCase("body-segment-1", "en-US", "segment-1", false, 1)]
    [TestCase("body-segment-2", "en-US", "segment-2", false, 1)]
    [TestCase("krop-segment-1", "da-DK", "segment-1", false, 1)]
    [TestCase("krop-segment-2", "da-DK", "segment-2", false, 1)]
    [TestCase("ボディ-segment-1", "ja-JP", "segment-1", false, 1)]
    [TestCase("ボディ-segment-2", "ja-JP", "segment-2", false, 1)]
    public async Task CanFilterByTextByCultureAndSegment(string text, string culture, string segment, bool negate, int expectedCount)
    {
        await CreateVariantDocument();

        var indexAlias = GetIndexAlias(false);

        SearchResult results = await Searcher.SearchAsync(
            indexAlias: indexAlias,
            filters: new List<Filter> { new TextFilter("body", [text], negate) },
            culture: culture,
            segment: segment);

        Assert.That(results.Total, Is.EqualTo(expectedCount));
    }

    [TestCase("Root", "en-US", true, 0)]
    [TestCase("Root", "en-US", false, 1)]
    [TestCase("Root", "da-DK", true, 0)]
    [TestCase("Root", "da-DK", false, 1)]
    [TestCase("Root", "ja-JP", true, 0)]
    [TestCase("Root", "ja-JP", false, 1)]
    public async Task CanSearchInvariantTextByCulture(string text, string culture, bool negate, int expectedCount)
    {
        await CreateVariantDocument();

        var indexAlias = GetIndexAlias(false);

        SearchResult results = await Searcher.SearchAsync(
            indexAlias: indexAlias,
            filters: new List<Filter> { new TextFilter("invariantTitle", [text], negate) },
            culture: culture);

        Assert.That(results.Total, Is.EqualTo(expectedCount));
    }

    public async Task CreateVariantDocument()
    {
        ILanguage langDk = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        ILanguage langJp = new LanguageBuilder()
            .WithCultureInfo("ja-JP")
            .Build();

        await LanguageService.CreateAsync(langDk, Constants.Security.SuperUserKey);
        await LanguageService.CreateAsync(langJp, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("variant")
            .WithContentVariation(ContentVariation.CultureAndSegment)
            .AddPropertyType()
            .WithAlias("title")
            .WithVariations(ContentVariation.Culture)
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("invariantTitle")
            .WithVariations(ContentVariation.Nothing)
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("body")
            .WithVariations(ContentVariation.CultureAndSegment)
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        Content root = new ContentBuilder()
            .WithKey(RootKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "Name")
            .WithCultureName("da-DK", "Navn")
            .WithCultureName("ja-JP", "名前")
            .Build();

        root.SetValue("invariantTitle", "Root");
        root.SetValue("title", "Root", "en-US");
        root.SetValue("title", "Rod", "da-DK");
        root.SetValue("title", "ル-ト", "ja-JP");

        root.SetValue("body", "body-segment-1", "en-US", "segment-1");
        root.SetValue("body", "body-segment-2", "en-US", "segment-2");
        root.SetValue("body", "krop-segment-1", "da-DK", "segment-1");
        root.SetValue("body", "krop-segment-2", "da-DK", "segment-2");
        root.SetValue("body", "ボディ-segment-1", "ja-JP", "segment-1");
        root.SetValue("body", "ボディ-segment-2", "ja-JP", "segment-2");

        await WaitForIndexing(GetIndexAlias(true), () =>
        {
            ContentService.Save(root);
            ContentService.Publish(root, ["*"]);

            return Task.CompletedTask;
        });

        IContent? content = ContentService.GetById(RootKey);
        Assert.That(content, Is.Not.Null);
    }
}
