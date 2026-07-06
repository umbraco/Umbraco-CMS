using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.SearchService;

public class VariantDocumentTests : SearcherTestBase
{
    [TestCase(true, "en-US", "Name")]
    [TestCase(false, "en-US", "Name")]
    [TestCase(true, "da-DK", "Navn")]
    [TestCase(false, "da-DK", "Navn")]
    [TestCase(true, "ja-JP", "名前")]
    [TestCase(false, "ja-JP", "名前")]
    public async Task CanSearchVariantName(bool publish, string culture, string expectedValue)
    {
        var indexAlias = GetIndexAlias(publish);

        SearchResult results = await Searcher.SearchAsync(indexAlias, expectedValue, null, null, null, culture, null, null, 0, 100);
        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(RootKey));
    }

    [TestCase(true, "en-US", "Name")]
    [TestCase(false, "en-US", "Name")]
    [TestCase(true, "da-DK", "Navn")]
    [TestCase(false, "da-DK", "Navn")]
    [TestCase(true, "ja-JP", "名前")]
    [TestCase(false, "ja-JP", "名前")]
    public async Task CanNotSearchVariantNameWithInvariantCulture(bool publish, string culture, string expectedValue)
    {
        var indexAlias = GetIndexAlias(publish);

        SearchResult results = await Searcher.SearchAsync(indexAlias, expectedValue, null, null, null, null, null, null, 0, 100);
        Assert.That(results.Total, Is.EqualTo(0));
    }

    [TestCase("title", "updated", "en-US")]
    [TestCase("title", "opdateret", "da-DK")]
    [TestCase("title", "ボディ", "ja-JP")]
    public async Task CanSearchUpdatedProperties(string propertyName, string updatedValue, string culture)
    {
        await UpdateProperty(propertyName, updatedValue, culture);

        var indexAlias = GetIndexAlias(true);

        SearchResult results = await Searcher.SearchAsync(indexAlias, updatedValue, null, null, null, culture, null, null, 0, 100);
        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(RootKey));
    }

    [TestCase(true, "en-US", "Root")]
    [TestCase(false, "en-US", "Root")]
    [TestCase(true, "da-DK", "Roden")]
    [TestCase(false, "da-DK", "Roden")]
    [TestCase(true, "ja-JP", "ル-ト")]
    [TestCase(false, "ja-JP", "ル-ト")]
    public async Task CanSearchVariantTextByCulture(bool publish, string culture, string expectedValue)
    {
        var indexAlias = GetIndexAlias(publish);

        SearchResult results = await Searcher.SearchAsync(indexAlias, expectedValue, null, null, null, culture, null, null, 0, 100);
        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(RootKey));
    }

    [TestCase(true, "en-US", "segment-1", "bodySegment1")]
    [TestCase(false, "en-US", "segment-2", "bodySegment2")]
    [TestCase(true, "da-DK","segment-1", "kropSegment1")]
    [TestCase(false, "da-DK","segment-2", "kropSegment2")]
    [TestCase(true, "ja-JP", "segment-1", "ボディSegment1")]
    [TestCase(false, "ja-JP", "segment-2", "ボディSegment2")]
    public async Task CanSearchVariantTextBySegment(bool publish, string culture, string segment, string expectedValue)
    {
        var indexAlias = GetIndexAlias(publish);

        SearchResult results = await Searcher.SearchAsync(indexAlias, expectedValue, null, null, null, culture, segment, null, 0, 100);
        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(RootKey));
    }

    [TestCase(true, "da-DK", "Roden")]
    [TestCase(true, "ja-JP", "ル-ト")]
    public async Task CannotSearchNonExistingNameAfterLanguageDelete(bool publish, string culture, string name)
    {
        var indexAlias = GetIndexAlias(publish);

        SearchResult results = await Searcher.SearchAsync(indexAlias, name, null, null, null, culture, null, null, 0, 100);
        Assert.That(results.Total, Is.EqualTo(1));

        await LanguageService.DeleteAsync(culture, Constants.Security.SuperUserKey);

        // We can't wait for indexing here, as it's an entire rebuild, not just a single action.
        await Task.Delay(4000);

        results = await Searcher.SearchAsync(indexAlias, name, null, null, null, culture, null, null, 0, 100);
        Assert.That(results.Total, Is.EqualTo(0));
    }

    [SetUp]
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

        root.SetValue("title", "Root", "en-US");
        root.SetValue("title", "Roden", "da-DK");
        root.SetValue("title", "ル-ト", "ja-JP");

        root.SetValue("body", "bodySegment1", "en-US", "segment-1");
        root.SetValue("body", "bodySegment2", "en-US", "segment-2");
        root.SetValue("body", "kropSegment1", "da-DK", "segment-1");
        root.SetValue("body", "kropSegment2", "da-DK", "segment-2");
        root.SetValue("body", "ボディSegment1", "ja-JP", "segment-1");
        root.SetValue("body", "ボディSegment2", "ja-JP", "segment-2");

        await WaitForIndexing(GetIndexAlias(true), () =>
        {
            ContentService.Save(root);
            ContentService.Publish(root, ["*"]);
            return Task.CompletedTask;
        });

        IContent? content = ContentService.GetById(RootKey);
        Assert.That(content, Is.Not.Null);
    }


    private async Task UpdateProperty(string propertyName, object value, string culture)
    {
        IContent content = ContentService.GetById(RootKey)!;
        content.SetValue(propertyName, value, culture);

        await WaitForIndexing(GetIndexAlias(true), () =>
        {
            ContentService.Save(content);
            ContentService.Publish(content, ["*"]);
            return Task.CompletedTask;
        });
    }
}
