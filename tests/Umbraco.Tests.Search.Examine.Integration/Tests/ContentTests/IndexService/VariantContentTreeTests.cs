using Examine;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.IndexService;

public class VariantContentTreeTests : IndexTestBase
{
    private const string EnglishRootTitle = "Root title";
    private const string DanishRootTitle = "Rod titel";
    private const string JapaneseRootTitle = "ル-トタイタル";
    private const string EnglishChildTitle = "Child title";
    private const string DanishChildTitle = "Barn titel";
    private const string JapaneseChildTitle = "子供タイタル";
    private const string EnglishGrandChildTitle = "Grandchild title";
    private const string DanishGrandChildTitle = "Barnebarn titel";
    private const string JapaneseGrandChildTitle = "孫タイタル";

    [Test]
    public async Task VariantStructure_YieldsAllDocuments()
    {
        await PublishEntireStructure();
        IIndex index = GetIndex(Cms.Core.Constants.IndexAliases.PublishedContent);

        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        // 3 documents (root, child, grandchild) x 3 cultures = 9 documents
        Assert.That(results.Count(), Is.EqualTo(3 * 3));
    }

    [Test]
    public async Task VariantStructure_WithRootUnpublished_YieldsNoDocuments()
    {
        await PublishEntireStructure();
        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            IContent root = ContentService.GetById(RootKey)!;
            ContentService.Unpublish(root);
            return Task.CompletedTask;
        });

        IIndex publishedIndex = GetIndex(Cms.Core.Constants.IndexAliases.PublishedContent);
        ISearchResults publishedResultsRootEnglish = publishedIndex.Searcher.Search(EnglishRootTitle);
        ISearchResults publishedResultsRootDanish = publishedIndex.Searcher.Search(DanishRootTitle);
        ISearchResults publishedResultsRootJapanese = publishedIndex.Searcher.Search(JapaneseRootTitle);
        ISearchResults publishedResultsChildEnglish = publishedIndex.Searcher.Search(EnglishChildTitle);
        ISearchResults publishedResultsChildDanish = publishedIndex.Searcher.Search(DanishChildTitle);
        ISearchResults publishedResultsChildJapanese = publishedIndex.Searcher.Search(JapaneseChildTitle);
        ISearchResults publishedResultsGrandChildEnglish = publishedIndex.Searcher.Search(EnglishGrandChildTitle);
        ISearchResults publishedResultsGrandChildDanish = publishedIndex.Searcher.Search(DanishGrandChildTitle);
        ISearchResults publishedResultsGrandChildJapanese = publishedIndex.Searcher.Search(JapaneseGrandChildTitle);
        Assert.Multiple(() =>
        {
            Assert.That(publishedResultsRootEnglish, Is.Empty);
            Assert.That(publishedResultsRootDanish, Is.Empty);
            Assert.That(publishedResultsRootJapanese, Is.Empty);
            Assert.That(publishedResultsChildEnglish, Is.Empty);
            Assert.That(publishedResultsChildDanish, Is.Empty);
            Assert.That(publishedResultsChildJapanese, Is.Empty);
            Assert.That(publishedResultsGrandChildEnglish, Is.Empty);
            Assert.That(publishedResultsGrandChildDanish, Is.Empty);
            Assert.That(publishedResultsGrandChildJapanese, Is.Empty);
        });
    }

    [Test]
    public async Task VariantStructure_WithChildUnpublished_YieldsNoDocumentsBelowRoot()
    {
        await PublishEntireStructure();
        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            IContent child = ContentService.GetById(ChildKey)!;
            ContentService.Unpublish(child);
            return Task.CompletedTask;
        });

        IIndex publishedIndex = GetIndex(Cms.Core.Constants.IndexAliases.PublishedContent);
        ISearchResults publishedResultsRootEnglish = publishedIndex.Searcher.Search(EnglishRootTitle);
        ISearchResults publishedResultsRootDanish = publishedIndex.Searcher.Search(DanishRootTitle);
        ISearchResults publishedResultsRootJapanese = publishedIndex.Searcher.Search(JapaneseRootTitle);
        ISearchResults publishedResultsChildEnglish = publishedIndex.Searcher.Search(EnglishChildTitle);
        ISearchResults publishedResultsChildDanish = publishedIndex.Searcher.Search(DanishChildTitle);
        ISearchResults publishedResultsChildJapanese = publishedIndex.Searcher.Search(JapaneseChildTitle);
        ISearchResults publishedResultsGrandChildEnglish = publishedIndex.Searcher.Search(EnglishGrandChildTitle);
        ISearchResults publishedResultsGrandChildDanish = publishedIndex.Searcher.Search(DanishGrandChildTitle);
        ISearchResults publishedResultsGrandChildJapanese = publishedIndex.Searcher.Search(JapaneseGrandChildTitle);
        Assert.Multiple(() =>
        {
            Assert.That(publishedResultsRootEnglish, Is.Not.Empty);
            Assert.That(publishedResultsRootDanish, Is.Not.Empty);
            Assert.That(publishedResultsRootJapanese, Is.Not.Empty);
            Assert.That(publishedResultsChildEnglish, Is.Empty);
            Assert.That(publishedResultsChildDanish, Is.Empty);
            Assert.That(publishedResultsChildJapanese, Is.Empty);
            Assert.That(publishedResultsGrandChildEnglish, Is.Empty);
            Assert.That(publishedResultsGrandChildDanish, Is.Empty);
            Assert.That(publishedResultsGrandChildJapanese, Is.Empty);
        });
    }

    [Test]
    public async Task VariantStructure_WithGrandChildUnpublished_YieldsNoDocumentsBelowChild()
    {
        await PublishEntireStructure();
        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            IContent grandChild = ContentService.GetById(GrandchildKey)!;
            ContentService.Unpublish(grandChild);
            return Task.CompletedTask;
        });

        IIndex publishedIndex = GetIndex(Cms.Core.Constants.IndexAliases.PublishedContent);
        ISearchResults publishedResultsRootEnglish = publishedIndex.Searcher.Search(EnglishRootTitle);
        ISearchResults publishedResultsRootDanish = publishedIndex.Searcher.Search(DanishRootTitle);
        ISearchResults publishedResultsRootJapanese = publishedIndex.Searcher.Search(JapaneseRootTitle);
        ISearchResults publishedResultsChildEnglish = publishedIndex.Searcher.Search(EnglishChildTitle);
        ISearchResults publishedResultsChildDanish = publishedIndex.Searcher.Search(DanishChildTitle);
        ISearchResults publishedResultsChildJapanese = publishedIndex.Searcher.Search(JapaneseChildTitle);
        ISearchResults publishedResultsGrandChildEnglish = publishedIndex.Searcher.Search(EnglishGrandChildTitle);
        ISearchResults publishedResultsGrandChildDanish = publishedIndex.Searcher.Search(DanishGrandChildTitle);
        ISearchResults publishedResultsGrandChildJapanese = publishedIndex.Searcher.Search(JapaneseGrandChildTitle);
        Assert.Multiple(() =>
        {
            Assert.That(publishedResultsRootEnglish, Is.Not.Empty);
            Assert.That(publishedResultsRootDanish, Is.Not.Empty);
            Assert.That(publishedResultsRootJapanese, Is.Not.Empty);
            Assert.That(publishedResultsChildEnglish, Is.Not.Empty);
            Assert.That(publishedResultsChildDanish, Is.Not.Empty);
            Assert.That(publishedResultsChildJapanese, Is.Not.Empty);
            Assert.That(publishedResultsGrandChildEnglish, Is.Empty);
            Assert.That(publishedResultsGrandChildDanish, Is.Empty);
            Assert.That(publishedResultsGrandChildJapanese, Is.Empty);
        });
    }

    [TestCase("en-US")]
    [TestCase("da-DK")]
    [TestCase("ja-JP")]
    public async Task PublishedStructureSingleCulture_YieldsAllPublishedDocumentsInOneCultures(string culture)
    {
        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            IContent root = ContentService.GetById(RootKey)!;
            ContentService.PublishBranch(root, PublishBranchFilter.IncludeUnpublished, [culture]);
            return Task.CompletedTask;
        });

        VerifyVariance([culture]);
    }


    [TestCase("en-US", "da-DK", "ja-JP")]
    [TestCase("da-DK", "en-US", "ja-JP")]
    [TestCase("ja-JP", "en-US", "da-DK")]
    public async Task PublishedStructureInAllCultures_WithUnpublishedRootInSingleCulture_YieldsAllDocumentInPublishedRootCulture(string cultureToUnpublish, string expectedCulture, string otherExpectedCulture)
    {
        await PublishEntireStructure();
        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            IContent root = ContentService.GetById(RootKey)!;
            PublishResult result = ContentService.Unpublish(root, cultureToUnpublish);
            Assert.That(result.Success, Is.True);
            return Task.CompletedTask;
        });

        VerifyVariance([expectedCulture, otherExpectedCulture]);
    }

    private void VerifyVariance(IEnumerable<string> expectedExistingCultures)
    {
        // Dictionary to map culture to expected root and child titles
        var rootTitles = new Dictionary<string, string>
        {
            { "en-US", EnglishRootTitle }, { "da-DK", DanishRootTitle }, { "ja-JP", JapaneseRootTitle },
        };

        var childTitles = new Dictionary<string, string>
        {
            { "en-US", EnglishChildTitle }, { "da-DK", DanishChildTitle }, { "ja-JP", JapaneseChildTitle },
        };

        var grandChildTitles = new Dictionary<string, string>
        {
            { "en-US", EnglishGrandChildTitle }, { "da-DK", DanishGrandChildTitle }, { "ja-JP", JapaneseGrandChildTitle }
        };

        var allCultures = new[] { "en-US", "da-DK", "ja-JP" };
        var expectedSet = new HashSet<string>(expectedExistingCultures);

        IIndex publishedIndex = GetIndex(Cms.Core.Constants.IndexAliases.PublishedContent);

        Assert.Multiple(() =>
        {
            foreach (var currentCulture in allCultures)
            {
                ISearchResults rootResults = publishedIndex.Searcher.Search(rootTitles[currentCulture]);
                ISearchResults childResults = publishedIndex.Searcher.Search(childTitles[currentCulture]);
                ISearchResults grandChildResults = publishedIndex.Searcher.Search(grandChildTitles[currentCulture]);

                if (expectedSet.Contains(currentCulture))
                {
                    Assert.That(rootResults, Is.Not.Empty);
                    Assert.That(childResults, Is.Not.Empty);
                    Assert.That(grandChildResults, Is.Not.Empty);
                }
                else
                {
                    Assert.That(rootResults, Is.Empty, $"Expected no root results for culture '{currentCulture}'");
                    Assert.That(childResults, Is.Empty, $"Expected no child results for culture '{currentCulture}'");
                    Assert.That(grandChildResults, Is.Empty, $"Expected no grandchild results for culture '{currentCulture}'");
                }
            }
        });
    }

    [SetUp]
    public async Task CreateVariantDocumentTree()
    {
        ILanguage langDk = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .WithIsDefault(true)
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
        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        Content root = new ContentBuilder()
            .WithKey(RootKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "Root")
            .WithCultureName("da-DK", "Rod")
            .WithCultureName("ja-JP", "ル-ト")
            .Build();

        root.SetValue("title", EnglishRootTitle, "en-US");
        root.SetValue("title", DanishRootTitle, "da-DK");
        root.SetValue("title", JapaneseRootTitle, "ja-JP");

        root.SetValue("body", "root-body-segment-1", "en-US", "segment-1");
        root.SetValue("body", "root-body-segment-2", "en-US", "segment-2");
        root.SetValue("body", "rod-krop-segment-1", "da-DK", "segment-1");
        root.SetValue("body", "rod-krop-segment-2", "da-DK", "segment-2");
        root.SetValue("body", "ル-ト-ボディ-segment-1", "ja-JP", "segment-1");
        root.SetValue("body", "ル-ト-ボディ-segment-2", "ja-JP", "segment-2");

        ContentService.Save(root);

        Content child = new ContentBuilder()
            .WithKey(ChildKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "Child")
            .WithCultureName("da-DK", "Barn")
            .WithCultureName("ja-JP", "子供")
            .WithParent(root)
            .Build();

        child.SetValue("title", EnglishChildTitle, "en-US");
        child.SetValue("title", DanishChildTitle, "da-DK");
        child.SetValue("title", JapaneseChildTitle, "ja-JP");

        child.SetValue("body", "child-body-segment-1", "en-US", "segment-1");
        child.SetValue("body", "child-body-segment-2", "en-US", "segment-2");
        child.SetValue("body", "barn-krop-segment-1", "da-DK", "segment-1");
        child.SetValue("body", "barn-krop-segment-2", "da-DK", "segment-2");
        child.SetValue("body", "子供-ボディ-segment-1", "ja-JP", "segment-1");
        child.SetValue("body", "子供-ボディ-segment-2", "ja-JP", "segment-2");

        ContentService.Save(child);

        Content grandchild = new ContentBuilder()
            .WithKey(GrandchildKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "Grandchild")
            .WithCultureName("da-DK", "Barn")
            .WithCultureName("ja-JP", "孫")
            .WithParent(child)
            .Build();

        grandchild.SetValue("title", EnglishGrandChildTitle, "en-US");
        grandchild.SetValue("title", DanishGrandChildTitle, "da-DK");
        grandchild.SetValue("title", JapaneseGrandChildTitle, "ja-JP");

        grandchild.SetValue("body", "grandchild-body-segment-1", "en-US", "segment-1");
        grandchild.SetValue("body", "grandchild-body-segment-2", "en-US", "segment-2");
        grandchild.SetValue("body", "barnebarn-krop-segment-1", "da-DK", "segment-1");
        grandchild.SetValue("body", "barnebarn-krop-segment-2", "da-DK", "segment-2");
        grandchild.SetValue("body", "孫-ボディ-segment-1", "ja-JP", "segment-1");
        grandchild.SetValue("body", "孫-ボディ-segment-2", "ja-JP", "segment-2");

        await WaitForIndexing(Cms.Core.Constants.IndexAliases.DraftContent, () =>
        {
            ContentService.Save(grandchild);
            return Task.CompletedTask;
        });
    }

    private async Task PublishEntireStructure() =>
        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            IContent root = ContentService.GetById(RootKey)!;
            ContentService.PublishBranch(root, PublishBranchFilter.IncludeUnpublished, ["*"]);
            return Task.CompletedTask;
        });
}
