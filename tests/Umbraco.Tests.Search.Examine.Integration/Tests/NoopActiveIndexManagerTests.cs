using Examine;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Search.Core;
using Umbraco.Cms.Search.Provider.Examine.Services;
using Umbraco.Tests.Search.Examine.Integration.Extensions;
using Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.IndexService;

namespace Umbraco.Tests.Search.Examine.Integration.Tests;

[TestFixture]
public class NoopActiveIndexManagerTests
{
    private ServiceProvider _serviceProvider;
    private IActiveIndexManager _activeIndexManager;
    private IExamineManager _examineManager;

    private const string IndexAlias = Constants.IndexAliases.PublishedContent;

    [SetUp]
    public void SetUp()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddExamineSearchProviderServicesWithoutZeroDowntimeForTest<TestIndex, TestInMemoryDirectoryFactory>()
            .AddLogging();

        _serviceProvider = serviceCollection.BuildServiceProvider();

        _activeIndexManager = _serviceProvider.GetRequiredService<IActiveIndexManager>();
        _examineManager = _serviceProvider.GetRequiredService<IExamineManager>();
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            _serviceProvider.Dispose();
        }
        catch (NullReferenceException)
        {
            // TestInMemoryDirectoryFactory.Dispose may throw if no index directories were created
        }
    }

    [Test]
    public void ResolveActiveIndexName_ReturnsAliasDirect()
    {
        var activeIndexName = _activeIndexManager.ResolveActiveIndexName(IndexAlias);

        Assert.That(activeIndexName, Is.EqualTo(IndexAlias));
    }

    [Test]
    public void ResolveShadowIndexName_ReturnsAliasDirect()
    {
        var shadowIndexName = _activeIndexManager.ResolveShadowIndexName(IndexAlias);

        Assert.That(shadowIndexName, Is.EqualTo(IndexAlias));
    }

    [Test]
    public void IsRebuilding_AlwaysReturnsFalse()
    {
        _activeIndexManager.StartRebuilding(IndexAlias);

        Assert.That(_activeIndexManager.IsRebuilding(IndexAlias), Is.False);
    }

    [Test]
    public void RebuildOperations_AreNoOps()
    {
        var activeBefore = _activeIndexManager.ResolveActiveIndexName(IndexAlias);

        _activeIndexManager.StartRebuilding(IndexAlias);
        _activeIndexManager.CompleteRebuilding(IndexAlias);

        var activeAfter = _activeIndexManager.ResolveActiveIndexName(IndexAlias);

        Assert.That(activeAfter, Is.EqualTo(activeBefore));
    }

    [Test]
    public void SingleIndexIsRegisteredPerAlias()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_examineManager.TryGetIndex(IndexAlias, out _), Is.True, "Single index should be registered for the alias");
            Assert.That(_examineManager.TryGetIndex(IndexAlias + ActiveIndexManager.SuffixA, out _), Is.False, "No _a suffix index should exist");
            Assert.That(_examineManager.TryGetIndex(IndexAlias + ActiveIndexManager.SuffixB, out _), Is.False, "No _b suffix index should exist");
        });
    }

    [Test]
    public void AllIndexAliasesAreRegistered()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_examineManager.TryGetIndex(Constants.IndexAliases.DraftContent, out _), Is.True);
            Assert.That(_examineManager.TryGetIndex(Constants.IndexAliases.PublishedContent, out _), Is.True);
            Assert.That(_examineManager.TryGetIndex(Constants.IndexAliases.DraftMedia, out _), Is.True);
            Assert.That(_examineManager.TryGetIndex(Constants.IndexAliases.DraftMembers, out _), Is.True);
        });
    }
}
