using Umbraco.Cms.Core;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DistributedContentIndexRefresherContentTests : TestBase
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IDistributedContentIndexRefresher DistributedContentIndexRefresher => GetRequiredService<IDistributedContentIndexRefresher>();

    private Guid _variantContentKey;

    private Guid _invariantContentKey;

    [SetUp]
    public async Task SetupTest()
    {
        await GetRequiredService<ILanguageService>().CreateAsync(
            new LanguageBuilder().WithCultureInfo("da-DK").Build(),
            Constants.Security.SuperUserKey);

        IContentType variantContentType = new ContentTypeBuilder()
            .WithAlias("variant")
            .WithContentVariation(ContentVariation.CultureAndSegment)
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(variantContentType, Constants.Security.SuperUserKey);

        IContentType invariantContentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(invariantContentType, Constants.Security.SuperUserKey);

        _variantContentKey = Guid.NewGuid();
        IContent variantContent = new ContentBuilder()
            .WithKey(_variantContentKey)
            .WithContentType(variantContentType)
            .WithCultureName("en-US", "Variant EN")
            .WithCultureName("da-DK", "Variant DA")
            .Build();
        ContentService.Save(variantContent);
        ContentService.Publish(variantContent, ["en-US", "da-DK"]);

        _invariantContentKey = Guid.NewGuid();
        IContent invariantContent = new ContentBuilder()
            .WithKey(_invariantContentKey)
            .WithContentType(invariantContentType)
            .WithName("Invariant")
            .Build();
        ContentService.Save(invariantContent);
        ContentService.Publish(invariantContent, ["*"]);

        IndexerAndSearcher.Reset();
    }

    [Test]
    public void RefreshContent_SingleDraft()
    {
        Assert.That(IndexerAndSearcher.Dump(IndexAliases.DraftContent), Is.Empty);

        DistributedContentIndexRefresher.RefreshContent([InvariantContent()], ContentState.Draft);

        IReadOnlyList<TestIndexDocument> dump = IndexerAndSearcher.Dump(IndexAliases.DraftContent);

        Assert.That(dump, Has.Count.EqualTo(1));
        Assert.That(dump[0].Id, Is.EqualTo(_invariantContentKey));
    }

    [Test]
    public void RefreshContent_MultipleDraft()
    {
        Assert.That(IndexerAndSearcher.Dump(IndexAliases.DraftContent), Is.Empty);

        DistributedContentIndexRefresher.RefreshContent([InvariantContent(), VariantContent()], ContentState.Draft);

        IReadOnlyList<TestIndexDocument> dump = IndexerAndSearcher.Dump(IndexAliases.DraftContent);

        Assert.That(dump, Has.Count.EqualTo(2));
        Assert.That(dump.Select(d => d.Id), Is.EquivalentTo(new[] { _invariantContentKey, _variantContentKey }));
    }

    [Test]
    public void RefreshContent_SinglePublished()
    {
        Assert.That(IndexerAndSearcher.Dump(IndexAliases.PublishedContent), Is.Empty);

        DistributedContentIndexRefresher.RefreshContent([InvariantContent()], ContentState.Published);

        IReadOnlyList<TestIndexDocument> dump = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);

        Assert.That(dump, Has.Count.EqualTo(1));
        Assert.That(dump[0].Id, Is.EqualTo(_invariantContentKey));
    }

    [Test]
    public void RefreshContent_MultiplePublished()
    {
        Assert.That(IndexerAndSearcher.Dump(IndexAliases.PublishedContent), Is.Empty);

        DistributedContentIndexRefresher.RefreshContent([InvariantContent(), VariantContent()], ContentState.Published);

        IReadOnlyList<TestIndexDocument> dump = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);

        Assert.That(dump, Has.Count.EqualTo(2));
        Assert.That(dump.Select(d => d.Id), Is.EquivalentTo(new[] { _invariantContentKey, _variantContentKey }));
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void RefreshContent_SinglePublished_SpecificLanguageVariants(bool publishEnglish, bool publishDanish)
    {
        ContentService.Unpublish(VariantContent());

        var culturesToPublish = new List<string>();
        if (publishEnglish)
        {
            culturesToPublish.Add("en-US");
        }
        if (publishDanish)
        {
            culturesToPublish.Add("da-DK");
        }
        ContentService.Publish(VariantContent(), culturesToPublish.ToArray());

        IndexerAndSearcher.Reset();
        Assert.That(IndexerAndSearcher.Dump(IndexAliases.PublishedContent), Is.Empty);

        DistributedContentIndexRefresher.RefreshContent([VariantContent()], ContentState.Published);

        IReadOnlyList<TestIndexDocument> dump = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);

        Assert.That(dump, Has.Count.EqualTo(1));
        Assert.That(dump[0].Id, Is.EqualTo(_variantContentKey));
        Assert.That(dump[0].Variations.Any(v => v.Culture == "en-US"), Is.EqualTo(publishEnglish));
        Assert.That(dump[0].Variations.Any(v => v.Culture == "da-DK"), Is.EqualTo(publishDanish));
    }

    private IContent VariantContent() => ContentService.GetById(_variantContentKey) ?? throw new InvalidOperationException("Variant content was not found");

    private IContent InvariantContent() => ContentService.GetById(_invariantContentKey) ?? throw new InvalidOperationException("Invariant content was not found");
}
