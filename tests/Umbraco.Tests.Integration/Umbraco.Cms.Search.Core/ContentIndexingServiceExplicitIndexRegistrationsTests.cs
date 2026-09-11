using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Configuration;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Cms.Search.Core;

public class ContentIndexingServiceExplicitIndexRegistrationsTests : ContentIndexingServiceTestsBase
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.Services.AddTransient<TestIndexerAndSearcher>();
        builder.Services.AddTransient<TestContentChangeStrategy>(_ => Strategy);

        builder.Services.Configure<IndexOptions>(options =>
        {
            options.RegisterContentIndex<TestIndexerAndSearcher, TestIndexerAndSearcher, TestContentChangeStrategy>(global::Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent, UmbracoObjectTypes.Document);
            options.RegisterContentIndex<TestIndexerAndSearcher, TestIndexerAndSearcher, TestContentChangeStrategy>(global::Umbraco.Cms.Core.Constants.IndexAliases.DraftContent, UmbracoObjectTypes.Document);
        });
    }

    [Test]
    public void IndexesAreRegistered()
    {
        IContentIndexingService sut = GetRequiredService<IContentIndexingService>();
        sut.Handle([ContentChange.Document(Guid.NewGuid(), ChangeImpact.Refresh, ContentState.Published)], "origin");

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
}
