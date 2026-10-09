// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     A sync adopts the repository cache versions it read before processing the pending cache instructions, and only
///     once every pending instruction has been processed. A read arriving while instructions are being processed must
///     still find its cache out of date and sync itself, and a page of pending instructions that is full must leave the
///     versions unadopted so the next read continues.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class CacheSyncServiceVersionAdoptionTests : CacheSyncIntegrationTestBase
{
    // One more than the page CacheInstructionService processes per call.
    private const int MoreThanOnePage = 101;

    private IRepositoryCacheVersionService CacheVersionService => GetRequiredService<IRepositoryCacheVersionService>();

    private SyncProbe Probe => GetRequiredService<SyncProbe>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.Services.AddSingleton<SyncProbe>();
        builder.CacheRefreshers().Add<VersionProbeCacheRefresher>();
    }

    [SetUp]
    public void ResetProbe() => Probe.Reset();

    [Test]
    public void ReadDuringInlineSync_SeesCacheAsOutOfDate_UntilInstructionsAreProcessed()
    {
        InitialiseLocalContentCacheVersion();
        WriteRemoteCacheVersion(ContentCacheKey);
        DeliverRemoteInstructions(ProbeInstruction());

        CacheSyncService.SyncInternal(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(Probe.ObservedSynced, Is.EqualTo(new[] { false }), "A read during the inline sync saw the cache as synced before the instruction had been processed.");
            Assert.That(IsContentCacheSynced(), Is.True, "The versions were not adopted once the instructions had been processed.");
        });
    }

    [Test]
    public void ReadDuringFullSync_SeesCacheAsOutOfDate_UntilInstructionsAreProcessed()
    {
        InitialiseLocalContentCacheVersion();
        WriteRemoteCacheVersion(ContentCacheKey);
        DeliverRemoteInstructions(ProbeInstruction());

        CacheSyncService.SyncAll(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(Probe.ObservedSynced, Is.EqualTo(new[] { false, false }), "A read during the full sync saw the cache as synced before the instruction had been processed.");
            Assert.That(IsContentCacheSynced(), Is.True, "The versions were not adopted once the instructions had been processed.");
        });
    }

    [Test]
    public void InlineSync_DoesNotAdoptVersions_WhileMorePendingInstructionsRemain()
    {
        InitialiseLocalContentCacheVersion();
        for (var i = 0; i < MoreThanOnePage; i++)
        {
            DeliverRemoteInstructions(RecordingInstruction(i));
        }

        WriteRemoteCacheVersion(ContentCacheKey);

        CacheSyncService.SyncInternal(CancellationToken.None);
        Assume.That(Refreshes.RefreshInternalCount, Is.EqualTo(MoreThanOnePage - 1), "The first sync did not stop at a full page, so this test proves nothing.");
        var syncedAfterFullPage = IsContentCacheSynced();

        CacheSyncService.SyncInternal(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(syncedAfterFullPage, Is.False, "The versions were adopted while instructions were still pending.");
            Assert.That(Refreshes.RefreshInternalCount, Is.EqualTo(MoreThanOnePage), "The second sync did not process the remaining instruction.");
            Assert.That(IsContentCacheSynced(), Is.True, "The versions were not adopted once every instruction had been processed.");
        });
    }

    [Test]
    public void FullSync_DoesNotAdoptVersions_WhileMorePendingInstructionsRemain()
    {
        InitialiseLocalContentCacheVersion();
        for (var i = 0; i < MoreThanOnePage; i++)
        {
            DeliverRemoteInstructions(RecordingInstruction(i));
        }

        WriteRemoteCacheVersion(ContentCacheKey);

        CacheSyncService.SyncAll(CancellationToken.None);
        Assume.That(Refreshes.RefreshCount, Is.EqualTo(MoreThanOnePage - 1), "The first sync did not stop at a full page, so this test proves nothing.");
        var syncedAfterFullPage = IsContentCacheSynced();

        CacheSyncService.SyncAll(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(syncedAfterFullPage, Is.False, "The versions were adopted while instructions were still pending.");
            Assert.That(Refreshes.RefreshCount, Is.EqualTo(MoreThanOnePage), "The second sync did not process the remaining instruction.");
            Assert.That(IsContentCacheSynced(), Is.True, "The versions were not adopted once every instruction had been processed.");
        });
    }

    [Test]
    public void CancelledSync_DoesNotAdoptVersions()
    {
        InitialiseLocalContentCacheVersion();
        WriteRemoteCacheVersion(ContentCacheKey);
        DeliverRemoteInstructions(RecordingInstruction());
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();

        CacheSyncService.SyncInternal(cancelled.Token);

        Assert.Multiple(() =>
        {
            Assert.That(Refreshes.RefreshInternalCount, Is.EqualTo(0), "A cancelled sync processed instructions.");
            Assert.That(IsContentCacheSynced(), Is.False, "A cancelled sync adopted the versions.");
        });
    }

    private bool IsContentCacheSynced()
        => CacheVersionService.IsCacheSyncedAsync<IContent>().GetAwaiter().GetResult();

    private static RefreshInstruction ProbeInstruction()
        => new(VersionProbeCacheRefresher.UniqueId, RefreshMethodType.RefreshByJson, Guid.Empty, 0, null!, "[]");

    /// <summary>Records what a repository read would see while a refresher runs: whether the IContent cache counts as synced.</summary>
    internal sealed class SyncProbe
    {
        private readonly ConcurrentQueue<bool> _observed = new();

        public IReadOnlyList<bool> ObservedSynced => _observed.ToList();

        public void Observe(bool synced) => _observed.Enqueue(synced);

        public void Reset() => _observed.Clear();
    }

    private sealed class VersionProbeCacheRefresher(IRepositoryCacheVersionService cacheVersionService, SyncProbe probe) : IJsonCacheRefresher
    {
        public static readonly Guid UniqueId = new("0B6E2F8C-5D3A-4E1B-9C7F-2A4D6E8F0B1C");

        public Guid RefresherUniqueId => UniqueId;

        public string Name => "Version probe cache refresher";

        public void RefreshAll()
        {
        }

        public void Refresh(int id)
        {
        }

        public void Remove(int id)
        {
        }

        public void Refresh(Guid id)
        {
        }

        public void Refresh(string json) => Observe();

        public void RefreshInternal(string json) => Observe();

        private void Observe() => probe.Observe(cacheVersionService.IsCacheSyncedAsync<IContent>().GetAwaiter().GetResult());
    }
}
