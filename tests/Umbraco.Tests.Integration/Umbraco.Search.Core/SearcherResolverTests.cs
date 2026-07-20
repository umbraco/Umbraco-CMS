using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Configuration;
using Umbraco.Cms.Search.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

[TestFixture]
internal class SearcherResolverTests : ResolverTestsBase<SearcherResolver>
{
    private ISearcherResolver SearcherResolver => GetRequiredService<ISearcherResolver>();

    private Mock<ILogger<SearcherResolver>>? _loggerMock;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.AddSearchCore();

        builder.Services.AddTransient<FirstSearcher>();
        builder.Services.AddTransient<SecondSearcher>();

        builder.Services.Configure<IndexOptions>(options =>
        {
            options.RegisterContentIndex<Indexer, FirstSearcher, TestContentChangeStrategy>("FirstIndex", UmbracoObjectTypes.Document);
            options.RegisterContentIndex<Indexer, SecondSearcher, TestContentChangeStrategy>("SecondIndex", UmbracoObjectTypes.Document);
            options.RegisterContentIndex<Indexer, UnregisteredSearcher, TestContentChangeStrategy>("IndexWithUnregisteredSearcher", UmbracoObjectTypes.Document);
        });

        _loggerMock = new Mock<ILogger<SearcherResolver>>();
        builder.Services.AddSingleton(_loggerMock.Object);
    }

    [Test]
    public void FirstIndex_ResolvesFirstSearcher()
    {
        ISearcher? searcher = SearcherResolver.GetSearcher("FirstIndex");
        Assert.That(searcher, Is.Not.Null);
        Assert.That(searcher, Is.TypeOf<FirstSearcher>());
    }

    [Test]
    public void SecondIndex_ResolvesSecondSearcher()
    {
        ISearcher? searcher = SearcherResolver.GetSearcher("SecondIndex");
        Assert.That(searcher, Is.Not.Null);
        Assert.That(searcher, Is.TypeOf<SecondSearcher>());
    }

    [Test]
    public void UnknownIndex_ResolvesNoSearcher()
    {
        ISearcher? searcher = SearcherResolver.GetSearcher("UnknownIndex");
        Assert.That(searcher, Is.Null);
        VerifyLogging(LogLevel.Warning, "No index registration was found");
    }

    [Test]
    public void UnregisteredSearcher_ResolvesNoSearcher()
    {
        ISearcher? searcher = SearcherResolver.GetSearcher("IndexWithUnregisteredSearcher");
        Assert.That(searcher, Is.Null);
        VerifyLogging(LogLevel.Error, "Could not resolve type");
    }

    private void VerifyLogging(LogLevel logLevel, string startOfMessage)
        => VerifyLogging(_loggerMock!, logLevel, startOfMessage);

    private class FirstSearcher : SearcherBase
    {
    }

    private class SecondSearcher : SearcherBase
    {
    }

    private class UnregisteredSearcher : SearcherBase
    {
    }

    private class Indexer : IndexerBase
    {
    }
}
