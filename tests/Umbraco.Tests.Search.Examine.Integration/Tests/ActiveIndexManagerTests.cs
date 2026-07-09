using Examine;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Search.Core;
using Umbraco.Cms.Search.Provider.Examine.Services;
using Umbraco.Tests.Search.Examine.Integration.Extensions;
using Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.IndexService;

namespace Umbraco.Tests.Search.Examine.Integration.Tests;

[TestFixture]
public class ActiveIndexManagerTests
{
    private ServiceProvider _serviceProvider;
    private IActiveIndexManager _activeIndexManager;
    private IExamineManager _examineManager;

    private const string IndexAlias = Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent;

    [SetUp]
    public void SetUp()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddExamineSearchProviderServicesForTest<TestIndex, TestInMemoryDirectoryFactory>()
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
    public void ResolveActiveIndexName_DefaultsToSuffixA()
    {
        var activeIndexName = _activeIndexManager.ResolveActiveIndexName(IndexAlias);

        Assert.That(activeIndexName, Is.EqualTo(IndexAlias + ActiveIndexManager.SuffixA));
    }

    [Test]
    public void ResolveShadowIndexName_DefaultsToSuffixB()
    {
        var shadowIndexName = _activeIndexManager.ResolveShadowIndexName(IndexAlias);

        Assert.That(shadowIndexName, Is.EqualTo(IndexAlias + ActiveIndexManager.SuffixB));
    }

    [Test]
    public void IsRebuilding_WhenNotStarted_ReturnsFalse()
    {
        var isRebuilding = _activeIndexManager.IsRebuilding(IndexAlias);

        Assert.That(isRebuilding, Is.False);
    }

    [Test]
    public void IsRebuilding_ForUnknownAlias_ReturnsFalse()
    {
        var isRebuilding = _activeIndexManager.IsRebuilding("NonExistentIndex");

        Assert.That(isRebuilding, Is.False);
    }

    [Test]
    public void StartRebuilding_SetsIsRebuildingToTrue()
    {
        _activeIndexManager.StartRebuilding(IndexAlias);

        Assert.That(_activeIndexManager.IsRebuilding(IndexAlias), Is.True);
    }

    [Test]
    public void StartRebuilding_DoesNotSwapActiveIndex()
    {
        var activeBeforeRebuild = _activeIndexManager.ResolveActiveIndexName(IndexAlias);

        _activeIndexManager.StartRebuilding(IndexAlias);

        var activeDuringRebuild = _activeIndexManager.ResolveActiveIndexName(IndexAlias);
        Assert.That(activeDuringRebuild, Is.EqualTo(activeBeforeRebuild));
    }

    [Test]
    public void StartRebuilding_WhenAlreadyRebuilding_DoesNotChangeState()
    {
        _activeIndexManager.StartRebuilding(IndexAlias);
        var shadowDuringFirstRebuild = _activeIndexManager.ResolveShadowIndexName(IndexAlias);

        // Start again while already rebuilding
        _activeIndexManager.StartRebuilding(IndexAlias);

        var shadowDuringSecondRebuild = _activeIndexManager.ResolveShadowIndexName(IndexAlias);
        Assert.That(shadowDuringSecondRebuild, Is.EqualTo(shadowDuringFirstRebuild));
        Assert.That(_activeIndexManager.IsRebuilding(IndexAlias), Is.True);
    }

    [Test]
    public void CompleteRebuilding_SwapsActiveAndShadow()
    {
        var activeBeforeRebuild = _activeIndexManager.ResolveActiveIndexName(IndexAlias);
        var shadowBeforeRebuild = _activeIndexManager.ResolveShadowIndexName(IndexAlias);

        _activeIndexManager.StartRebuilding(IndexAlias);
        _activeIndexManager.CompleteRebuilding(IndexAlias);

        var activeAfterRebuild = _activeIndexManager.ResolveActiveIndexName(IndexAlias);
        var shadowAfterRebuild = _activeIndexManager.ResolveShadowIndexName(IndexAlias);

        Assert.Multiple(() =>
        {
            Assert.That(activeAfterRebuild, Is.EqualTo(shadowBeforeRebuild), "Active should swap to what was shadow");
            Assert.That(shadowAfterRebuild, Is.EqualTo(activeBeforeRebuild), "Shadow should swap to what was active");
            Assert.That(_activeIndexManager.IsRebuilding(IndexAlias), Is.False);
        });
    }

    [Test]
    public void CancelRebuilding_DoesNotSwap()
    {
        var activeBeforeRebuild = _activeIndexManager.ResolveActiveIndexName(IndexAlias);

        _activeIndexManager.StartRebuilding(IndexAlias);
        _activeIndexManager.CancelRebuilding(IndexAlias);

        var activeAfterCancel = _activeIndexManager.ResolveActiveIndexName(IndexAlias);

        Assert.Multiple(() =>
        {
            Assert.That(activeAfterCancel, Is.EqualTo(activeBeforeRebuild), "Active should remain the same after cancel");
            Assert.That(_activeIndexManager.IsRebuilding(IndexAlias), Is.False);
        });
    }

    [Test]
    public void CompleteRebuilding_WhenNotRebuilding_DoesNotSwap()
    {
        var activeBeforeComplete = _activeIndexManager.ResolveActiveIndexName(IndexAlias);

        _activeIndexManager.CompleteRebuilding(IndexAlias);

        var activeAfterComplete = _activeIndexManager.ResolveActiveIndexName(IndexAlias);
        Assert.That(activeAfterComplete, Is.EqualTo(activeBeforeComplete));
    }

    [Test]
    public void CancelRebuilding_WhenNotRebuilding_NoOp()
    {
        var activeBeforeCancel = _activeIndexManager.ResolveActiveIndexName(IndexAlias);

        _activeIndexManager.CancelRebuilding(IndexAlias);

        var activeAfterCancel = _activeIndexManager.ResolveActiveIndexName(IndexAlias);
        Assert.That(activeAfterCancel, Is.EqualTo(activeBeforeCancel));
    }

    [Test]
    public void MultipleRebuildCycles_SwapCorrectly()
    {
        // Cycle 1: _a -> _b
        var activeInitial = _activeIndexManager.ResolveActiveIndexName(IndexAlias);
        Assert.That(activeInitial, Does.EndWith(ActiveIndexManager.SuffixA));

        _activeIndexManager.StartRebuilding(IndexAlias);
        _activeIndexManager.CompleteRebuilding(IndexAlias);

        var activeAfterFirstSwap = _activeIndexManager.ResolveActiveIndexName(IndexAlias);
        Assert.That(activeAfterFirstSwap, Does.EndWith(ActiveIndexManager.SuffixB));

        // Cycle 2: _b -> _a
        _activeIndexManager.StartRebuilding(IndexAlias);
        _activeIndexManager.CompleteRebuilding(IndexAlias);

        var activeAfterSecondSwap = _activeIndexManager.ResolveActiveIndexName(IndexAlias);
        Assert.That(activeAfterSecondSwap, Does.EndWith(ActiveIndexManager.SuffixA));
    }

    [Test]
    public void DifferentIndexAliases_HaveIndependentState()
    {
        var publishedAlias = Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent;
        var draftAlias = Umbraco.Cms.Core.Constants.IndexAliases.DraftContent;

        _activeIndexManager.StartRebuilding(publishedAlias);

        Assert.Multiple(() =>
        {
            Assert.That(_activeIndexManager.IsRebuilding(publishedAlias), Is.True);
            Assert.That(_activeIndexManager.IsRebuilding(draftAlias), Is.False);
        });
    }

    [Test]
    public void DifferentIndexAliases_SwapIndependently()
    {
        var publishedAlias = Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent;
        var draftAlias = Umbraco.Cms.Core.Constants.IndexAliases.DraftContent;

        // Swap published only
        _activeIndexManager.StartRebuilding(publishedAlias);
        _activeIndexManager.CompleteRebuilding(publishedAlias);

        Assert.Multiple(() =>
        {
            Assert.That(_activeIndexManager.ResolveActiveIndexName(publishedAlias), Does.EndWith(ActiveIndexManager.SuffixB));
            Assert.That(_activeIndexManager.ResolveActiveIndexName(draftAlias), Does.EndWith(ActiveIndexManager.SuffixA));
        });
    }

    [Test]
    public void DetermineInitialSlot_WhenOnlySlotBHasData_SelectsB()
    {
        // Index data into the _b slot BEFORE constructing ActiveIndexManager,
        // since it now eagerly determines initial slots in its constructor.
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddExamineSearchProviderServicesForTest<TestIndex, TestInMemoryDirectoryFactory>()
            .AddLogging();

        // Build without resolving IActiveIndexManager yet
        using var sp = serviceCollection.BuildServiceProvider();
        var examineManager = sp.GetRequiredService<IExamineManager>();

        var bIndexName = IndexAlias + ActiveIndexManager.SuffixB;
        IIndex bIndex = examineManager.GetIndex(bIndexName);
        bIndex.IndexItem(new ValueSet("test-1", "Document", new Dictionary<string, object> { { "name", "Test" } }));

        Thread.Sleep(3000);

        // Now resolve ActiveIndexManager — constructor will see _b has data
        var activeIndexManager = sp.GetRequiredService<IActiveIndexManager>();
        var activeIndexName = activeIndexManager.ResolveActiveIndexName(IndexAlias);

        Assert.That(activeIndexName, Does.EndWith(ActiveIndexManager.SuffixB), "When only slot B has data, B should be selected as active");
    }

    [Test]
    public void DetermineInitialSlot_WhenBothSlotsHaveData_SelectsSlotWithMoreDocuments()
    {
        // Index data into both slots BEFORE constructing ActiveIndexManager,
        // since it now eagerly determines initial slots in its constructor.
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddExamineSearchProviderServicesForTest<TestIndex, TestInMemoryDirectoryFactory>()
            .AddLogging();

        // Build without resolving IActiveIndexManager yet
        using var sp = serviceCollection.BuildServiceProvider();
        var examineManager = sp.GetRequiredService<IExamineManager>();

        var aIndexName = IndexAlias + ActiveIndexManager.SuffixA;
        var bIndexName = IndexAlias + ActiveIndexManager.SuffixB;

        IIndex aIndex = examineManager.GetIndex(aIndexName);
        IIndex bIndex = examineManager.GetIndex(bIndexName);

        // _a gets 1 document
        aIndex.IndexItem(new ValueSet("a-1", "Document", new Dictionary<string, object> { { "name", "A1" } }));

        // _b gets 3 documents
        bIndex.IndexItem(new ValueSet("b-1", "Document", new Dictionary<string, object> { { "name", "B1" } }));
        bIndex.IndexItem(new ValueSet("b-2", "Document", new Dictionary<string, object> { { "name", "B2" } }));
        bIndex.IndexItem(new ValueSet("b-3", "Document", new Dictionary<string, object> { { "name", "B3" } }));

        Thread.Sleep(3000);

        // Now resolve ActiveIndexManager — constructor will see both slots have data
        var activeIndexManager = sp.GetRequiredService<IActiveIndexManager>();
        var activeIndexName = activeIndexManager.ResolveActiveIndexName(IndexAlias);

        Assert.That(activeIndexName, Does.EndWith(ActiveIndexManager.SuffixB), "When both slots have data, the slot with more documents should be selected as active");
    }

    [Test]
    public void ShadowIndexName_IsOppositeOfActive()
    {
        var active = _activeIndexManager.ResolveActiveIndexName(IndexAlias);
        var shadow = _activeIndexManager.ResolveShadowIndexName(IndexAlias);

        Assert.That(active, Is.Not.EqualTo(shadow), "Active and shadow should be different physical indexes");

        // After swap, they reverse
        _activeIndexManager.StartRebuilding(IndexAlias);
        _activeIndexManager.CompleteRebuilding(IndexAlias);

        Assert.Multiple(() =>
        {
            Assert.That(_activeIndexManager.ResolveActiveIndexName(IndexAlias), Is.EqualTo(shadow));
            Assert.That(_activeIndexManager.ResolveShadowIndexName(IndexAlias), Is.EqualTo(active));
        });
    }
}
