using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Configuration;
using Umbraco.Cms.Search.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.Services;

namespace Umbraco.Tests.Search.Integration.Tests;

[TestFixture]
internal class IndexerResolverTests : ResolverTestsBase<IndexerResolver>
{
    private IIndexerResolver IndexerResolver => GetRequiredService<IIndexerResolver>();

    private Mock<ILogger<IndexerResolver>>? _loggerMock;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.AddSearchCore();

        builder.Services.AddTransient<FirstIndexer>();
        builder.Services.AddTransient<SecondIndexer>();

        builder.Services.Configure<IndexOptions>(options =>
        {
            options.RegisterContentIndex<FirstIndexer, Searcher, TestContentChangeStrategy>("FirstIndex", UmbracoObjectTypes.Document);
            options.RegisterContentIndex<SecondIndexer, Searcher, TestContentChangeStrategy>("SecondIndex", UmbracoObjectTypes.Document);
            options.RegisterContentIndex<ThirdIndexer, Searcher, TestContentChangeStrategy>("IndexWithUnregisteredIndexer", UmbracoObjectTypes.Document);
        });

        _loggerMock = new Mock<ILogger<IndexerResolver>>();
        builder.Services.AddSingleton(_loggerMock.Object);
    }

    [Test]
    public void FirstIndex_ResolvesFirstIndexer()
    {
        IIndexer? indexer = IndexerResolver.GetIndexer("FirstIndex");
        Assert.That(indexer, Is.Not.Null);
        Assert.That(indexer, Is.TypeOf<FirstIndexer>());
    }

    [Test]
    public void SecondIndex_ResolvesSecondIndexer()
    {
        IIndexer? indexer = IndexerResolver.GetIndexer("SecondIndex");
        Assert.That(indexer, Is.Not.Null);
        Assert.That(indexer, Is.TypeOf<SecondIndexer>());
    }

    [Test]
    public void UnknownIndex_ResolvesNoIndexer()
    {
        IIndexer? indexer = IndexerResolver.GetIndexer("UnknownIndex");
        Assert.That(indexer, Is.Null);
        VerifyLogging(LogLevel.Warning, "No index registration was found");
    }

    [Test]
    public void UnregisteredIndexer_ResolvesNoIndexer()
    {
        IIndexer? indexer = IndexerResolver.GetIndexer("IndexWithUnregisteredIndexer");
        Assert.That(indexer, Is.Null);
        VerifyLogging(LogLevel.Error, "Could not resolve type");
    }

    private void VerifyLogging(LogLevel logLevel, string startOfMessage)
        => VerifyLogging(_loggerMock!, logLevel, startOfMessage);

    private class Searcher : SearcherBase
    {
    }

    private class FirstIndexer : IndexerBase
    {
    }

    private class SecondIndexer : IndexerBase
    {
    }

    private class ThirdIndexer : IndexerBase
    {
    }
}
