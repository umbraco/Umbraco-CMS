using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core;
using Umbraco.Cms.Search.Core.Configuration;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class ContentIndexingServiceImplicitIndexRegistrationsTests : ContentIndexingServiceTestsBase
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.Services.AddTransient<IIndexer, TestIndexerAndSearcher>();
        builder.Services.AddTransient<ISearcher, TestIndexerAndSearcher>();
        builder.Services.AddTransient<IPublishedContentChangeStrategy>(_ => Strategy);
        builder.Services.AddTransient<IDraftContentChangeStrategy>(_ => Strategy);

        builder.Services.Configure<IndexOptions>(options =>
        {
            options.RegisterContentIndex<IIndexer, ISearcher, IPublishedContentChangeStrategy>(global::Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent, UmbracoObjectTypes.Document);
            options.RegisterContentIndex<IIndexer, ISearcher, IDraftContentChangeStrategy>(global::Umbraco.Cms.Core.Constants.IndexAliases.DraftContent, UmbracoObjectTypes.Document);
        });
    }

    [Test]
    public void IndexesAreRegistered()
    {
        IContentIndexingService sut = GetRequiredService<IContentIndexingService>();
        sut.Handle([ContentChange.Document(Guid.NewGuid(), ChangeImpact.Refresh, ContentState.Published)], "origin");

        // two different change strategies registered (although it's the implementation)
        Assert.That(Strategy.HandledIndexInfos, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            // ...each invoked once
            Assert.That(Strategy.HandledIndexInfos[0], Has.Count.EqualTo(1));
            Assert.That(Strategy.HandledIndexInfos[1], Has.Count.EqualTo(1));
        });

        Assert.Multiple(() =>
        {
            Assert.That(Strategy.HandledIndexInfos[0][0].IndexAlias, Is.EqualTo(global::Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent));
            Assert.That(Strategy.HandledIndexInfos[0][0].Indexer, Is.TypeOf<TestIndexerAndSearcher>());

            Assert.That(Strategy.HandledIndexInfos[1][0].IndexAlias, Is.EqualTo(global::Umbraco.Cms.Core.Constants.IndexAliases.DraftContent));
            Assert.That(Strategy.HandledIndexInfos[1][0].Indexer, Is.TypeOf<TestIndexerAndSearcher>());
        });
    }
}
