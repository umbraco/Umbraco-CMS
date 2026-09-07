using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core;
using Umbraco.Cms.Search.Core.Configuration;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class ContentIndexingServiceSameOriginOnlyTests : ContentIndexingServiceTestsBase
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.Services.AddTransient<TestIndexerAndSearcher>();
        builder.Services.AddTransient<TestContentChangeStrategy>(_ => Strategy);

        var originProviderMock = new Mock<IOriginProvider>();
        originProviderMock.Setup(m => m.GetCurrent()).Returns("current-origin");
        builder.Services.AddUnique(originProviderMock.Object);

        builder.Services.Configure<IndexOptions>(options =>
        {
            options.RegisterContentIndex<TestIndexerAndSearcher, TestIndexerAndSearcher, TestContentChangeStrategy>(global::Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent, true, UmbracoObjectTypes.Document);
            options.RegisterContentIndex<TestIndexerAndSearcher, TestIndexerAndSearcher, TestContentChangeStrategy>(global::Umbraco.Cms.Core.Constants.IndexAliases.DraftContent, true, UmbracoObjectTypes.Document);
        });
    }

    [SetUp]
    public void SetupTest() => Strategy.HandledIndexInfos.Clear();

    [Test]
    public void ContentChangesAreIgnoredForOtherOrigin()
    {
        IContentIndexingService sut = GetRequiredService<IContentIndexingService>();
        sut.Handle([ContentChange.Document(Guid.NewGuid(), ChangeImpact.Refresh, ContentState.Published)], "other-origin");

        // no changes handled because the origin differs
        Assert.That(Strategy.HandledIndexInfos, Has.Count.EqualTo(0));
    }

    [Test]
    public void ContentChangesAreHandledForCurrentOrigin()
    {
        IContentIndexingService sut = GetRequiredService<IContentIndexingService>();
        sut.Handle([ContentChange.Document(Guid.NewGuid(), ChangeImpact.Refresh, ContentState.Published)], "current-origin");

        // one change strategy registered (same for both indexes)
        Assert.That(Strategy.HandledIndexInfos, Has.Count.EqualTo(1));
        // ...invoked twice
        Assert.That(Strategy.HandledIndexInfos[0], Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(Strategy.HandledIndexInfos[0][0].IndexAlias, Is.EqualTo(global::Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent));
            Assert.That(Strategy.HandledIndexInfos[0][0].Indexer, Is.TypeOf<TestIndexerAndSearcher>());

            Assert.That(Strategy.HandledIndexInfos[0][1].IndexAlias, Is.EqualTo(global::Umbraco.Cms.Core.Constants.IndexAliases.DraftContent));
            Assert.That(Strategy.HandledIndexInfos[0][1].Indexer, Is.TypeOf<TestIndexerAndSearcher>());
        });
    }

    [Test]
    public void IndexRebuildsAreIgnoredForOtherOrigin()
    {
        IContentIndexingService sut = GetRequiredService<IContentIndexingService>();
        sut.Rebuild(global::Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent, "other-origin");

        // no changes handled because the origin differs
        Assert.That(Strategy.HandledIndexInfos, Has.Count.EqualTo(0));
    }

    [Test]
    public void IndexRebuildsAreHandledForCurrentOrigin()
    {
        IContentIndexingService sut = GetRequiredService<IContentIndexingService>();
        sut.Rebuild(global::Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent, "current-origin");

        // one change strategy registered (same for both indexes)
        Assert.That(Strategy.HandledIndexInfos, Has.Count.EqualTo(1));
        // ...invoked once (a single rebuild)
        Assert.That(Strategy.HandledIndexInfos[0], Has.Count.EqualTo(1));

        Assert.Multiple(() =>
        {
            Assert.That(Strategy.HandledIndexInfos[0][0].IndexAlias, Is.EqualTo(global::Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent));
            Assert.That(Strategy.HandledIndexInfos[0][0].Indexer, Is.TypeOf<TestIndexerAndSearcher>());
        });
    }
}
