// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.DistributedLocking;
using Umbraco.Cms.Infrastructure.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Reproduces a lock-ordering problem with <c>LoadBalanceIsolatedCaches()</c>.
/// </summary>
/// <remarks>
///     <para>
///         Umbraco takes distributed locks lazily: <c>scope.WriteLock(ContentTree)</c> only queues the lock, and it
///         is acquired on the scope's first database access. When a repository read in that scope finds its isolated
///         cache out of date, <c>RepositoryCachePolicyBase.EnsureCacheIsSynced</c> calls
///         <see cref="ICacheSyncService.SyncInternal" />, which runs
///         <c>CacheInstructionService.ProcessInternalInstructions</c> under <c>_syncLock</c>. The scopes it creates
///         there are nested in the caller's scope, so their first database access acquires the caller's queued
///         ContentTree write lock while <c>_syncLock</c> is held.
///     </para>
///     <para>
///         That is the reverse of the order used by <c>InstructionProcessJob</c> (take <c>_syncLock</c>, then
///         ContentTree via <c>DocumentCacheService.RefreshMemoryCacheAsync</c>). With two or more servers under load
///         this shows up as ContentTree held for the whole sync plus the rest of the publish (observed: 33 s), and
///         every other content operation on every server timing out behind it.
///     </para>
///     <para>
///         The tests assert the desired behaviour, so they fail until the ordering is fixed.
///     </para>
/// </remarks>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class CacheSyncServiceLockOrderingTests : UmbracoIntegrationTestWithContent
{
    private ICacheSyncService CacheSyncService => GetRequiredService<ICacheSyncService>();

    private IRepositoryCacheVersionRepository CacheVersionRepository => GetRequiredService<IRepositoryCacheVersionRepository>();

    private IRepositoryCacheVersionAccessor CacheVersionAccessor => GetRequiredService<IRepositoryCacheVersionAccessor>();

    private LockRecorder Recorder => GetRequiredService<LockRecorder>();

    // Repositories only use (and sync) their cache policies with real app caches; the test host defaults to NoCache.
    // The request cache is a mock so the version is not cached across the whole test.
    protected override void ConfigureTestServices(IServiceCollection services)
        => services.AddSingleton(AppCaches.Create(Mock.Of<IRequestCache>()));

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        // Load-balanced isolated caches: repository reads compare their cache version with the database.
        builder.LoadBalanceIsolatedCaches();

        // Record every distributed lock that is obtained, and whether CacheInstructionService._syncLock was held.
        builder.Services.AddSingleton<LockRecorder>();
        builder.Services.AddUnique<IDistributedLockingMechanismFactory>(sp => new RecordingLockingMechanismFactory(
            ActivatorUtilities.CreateInstance<DefaultDistributedLockingMechanismFactory>(sp),
            sp.GetRequiredService<LockRecorder>(),
            sp));

        // Record every inline sync, and whether the caller already held ContentTree at that point.
        builder.Services.AddUnique<ICacheSyncService>(sp => new RecordingCacheSyncService(
            ActivatorUtilities.CreateInstance<CacheSyncService>(sp),
            sp.GetRequiredService<LockRecorder>(),
            sp.GetRequiredService<Cms.Infrastructure.Scoping.IScopeAccessor>()));
    }

    [Test]
    public void SyncInternal_Does_Not_Obtain_The_Callers_Queued_Locks_While_Holding_The_Sync_Lock()
    {
        Recorder.Clear();

        using (IScope scope = ScopeProvider.CreateScope())
        {
            // Queued only: acquired on this scope's first database access.
            scope.WriteLock(Constants.Locks.ContentTree);

            CacheSyncService.SyncInternal(CancellationToken.None);

            scope.Complete();
        }

        Assert.That(
            Recorder.ObtainedUnderSyncLock(Constants.Locks.ContentTree),
            Is.Empty,
            "The caller's queued ContentTree lock was obtained inside CacheInstructionService while _syncLock was held.");
    }

    [Test]
    public void SyncInternal_Does_Not_Wait_For_The_Sync_Lock_While_The_Caller_Holds_ContentTree()
    {
        // InstructionProcessJob holds _syncLock (in production: while it waits for ContentTree behind the caller).
        var syncLock = GetSyncLock();
        using var holding = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        Task holder = Task.Factory.StartNew(
            () =>
            {
                lock (syncLock)
                {
                    holding.Set();
                    release.Wait(TimeSpan.FromSeconds(30));
                }
            },
            TaskCreationOptions.LongRunning);
        holding.Wait();

        // A save or publish that already holds the ContentTree write lock finds its cache stale and syncs inline.
        Task caller = Task.Run(() =>
        {
            using IScope scope = ScopeProvider.CreateScope();
            scope.EagerWriteLock(Constants.Locks.ContentTree);
            CacheSyncService.SyncInternal(CancellationToken.None);
            scope.Complete();
        });

        var finishedWhileSyncLockHeld = caller.Wait(TimeSpan.FromSeconds(5));
        release.Set();
        holder.Wait();
        caller.Wait();

        Assert.That(
            finishedWhileSyncLockHeld,
            Is.True,
            "SyncInternal waited for _syncLock while the caller held ContentTree. If the thread holding _syncLock is waiting for ContentTree, that is a deadlock SQL Server cannot detect.");
    }

    [Test]
    public void Reading_Content_With_A_Stale_Cache_Does_Not_Obtain_ContentTree_While_Holding_The_Sync_Lock()
    {
        var cacheKey = typeof(IContent).FullName!;

        // This server has seen the current IContent cache version (a read initialises the local version).
        WriteRemoteCacheVersion(cacheKey);
        ContentService.GetById(Textpage.Key);

        // Another server saves a document: it writes a new IContent cache version to the database,
        // which this server's in-memory version does not know about.
        WriteRemoteCacheVersion(cacheKey);

        Recorder.Clear();

        // A publish on this server: it queues the ContentTree write lock and then reads the document.
        using (IScope scope = ScopeProvider.CreateScope())
        {
            // An earlier read in the same request/scope has already fetched the version (it is cached per
            // scope), so the version check itself does not touch the database here.
            CacheVersionAccessor.GetAsync(cacheKey).GetAwaiter().GetResult();

            scope.WriteLock(Constants.Locks.ContentTree);

            ContentService.GetById(Textpage.Key);

            scope.Complete();
        }

        Assume.That(Recorder.InlineSyncs, Is.Not.Empty, "The read did not trigger an inline cache sync, so this test proves nothing.");

        Assert.Multiple(() =>
        {
            // Variant 1: the caller already holds ContentTree and then waits for _syncLock.
            Assert.That(
                Recorder.InlineSyncs.Where(heldContentTree => heldContentTree),
                Is.Empty,
                "SyncInternal was called while the caller held ContentTree, so it waits for _syncLock while holding it.");

            // Variant 2: the sync obtains the caller's queued ContentTree lock while holding _syncLock.
            Assert.That(
                Recorder.ObtainedUnderSyncLock(Constants.Locks.ContentTree),
                Is.Empty,
                "ContentTree was obtained while CacheInstructionService._syncLock was held, which inverts the lock order used by InstructionProcessJob.");
        });
    }

    private Lock GetSyncLock()
        => (Lock)RecordingLockingMechanism.SyncLockField.GetValue(GetRequiredService<ICacheInstructionService>())!;

    /// <summary>Writes a cache version the way another server would: to the database only.</summary>
    private void WriteRemoteCacheVersion(string cacheKey)
    {
        using IScope scope = ScopeProvider.CreateScope();
        CacheVersionRepository.SaveAsync(new RepositoryCacheVersion
        {
            Identifier = cacheKey,
            Version = Guid.NewGuid().ToString(),
        }).GetAwaiter().GetResult();
        scope.Complete();
    }

    /// <summary>Records lock acquisitions and whether <c>CacheInstructionService._syncLock</c> was held.</summary>
    internal sealed class LockRecorder
    {
        private readonly ConcurrentQueue<(int LockId, DistributedLockType Type, bool UnderSyncLock)> _obtained = new();
        private readonly ConcurrentQueue<bool> _inlineSyncs = new();

        /// <summary>One entry per inline sync: whether the caller held ContentTree when it was called.</summary>
        public IReadOnlyList<bool> InlineSyncs => _inlineSyncs.ToList();

        public void Record(int lockId, DistributedLockType type, bool underSyncLock) => _obtained.Enqueue((lockId, type, underSyncLock));

        public void RecordInlineSync(bool callerHeldContentTree) => _inlineSyncs.Enqueue(callerHeldContentTree);

        public void Clear()
        {
            _obtained.Clear();
            _inlineSyncs.Clear();
        }

        public IReadOnlyList<DistributedLockType> ObtainedUnderSyncLock(int lockId)
            => _obtained.Where(x => x.LockId == lockId && x.UnderSyncLock).Select(x => x.Type).ToList();
    }

    private sealed class RecordingCacheSyncService(
        ICacheSyncService inner,
        LockRecorder recorder,
        Cms.Infrastructure.Scoping.IScopeAccessor scopeAccessor) : ICacheSyncService
    {
        public void SyncAll(CancellationToken cancellationToken) => inner.SyncAll(cancellationToken);

        public void SyncInternal(CancellationToken cancellationToken)
        {
            var heldContentTree = scopeAccessor.AmbientScope?.Locks.GetWriteLocks()?.Values
                .Any(byLockId => byLockId.TryGetValue(Constants.Locks.ContentTree, out var count) && count > 0) ?? false;
            recorder.RecordInlineSync(heldContentTree);
            inner.SyncInternal(cancellationToken);
        }
    }

    private sealed class RecordingLockingMechanismFactory(
        IDistributedLockingMechanismFactory inner,
        LockRecorder recorder,
        IServiceProvider serviceProvider) : IDistributedLockingMechanismFactory
    {
        private RecordingLockingMechanism? _mechanism;

        public IDistributedLockingMechanism DistributedLockingMechanism
            => _mechanism ??= new RecordingLockingMechanism(inner.DistributedLockingMechanism, recorder, serviceProvider);
    }

    private sealed class RecordingLockingMechanism(
        IDistributedLockingMechanism inner,
        LockRecorder recorder,
        IServiceProvider serviceProvider) : IDistributedLockingMechanism
    {
        internal static readonly FieldInfo SyncLockField =
            typeof(CacheInstructionService).GetField("_syncLock", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("CacheInstructionService._syncLock not found; update the test.");

        public bool Enabled => inner.Enabled;

        public IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null)
        {
            IDistributedLock obtained = inner.ReadLock(lockId, obtainLockTimeout);
            recorder.Record(lockId, DistributedLockType.ReadLock, SyncLockHeldByCurrentThread());
            return obtained;
        }

        public IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null)
        {
            IDistributedLock obtained = inner.WriteLock(lockId, obtainLockTimeout);
            recorder.Record(lockId, DistributedLockType.WriteLock, SyncLockHeldByCurrentThread());
            return obtained;
        }

        // Resolved lazily: CacheInstructionService depends on the scope provider, which depends on this factory.
        private bool SyncLockHeldByCurrentThread()
            => serviceProvider.GetRequiredService<ICacheInstructionService>() is CacheInstructionService service
               && SyncLockField.GetValue(service) is Lock syncLock
               && syncLock.IsHeldByCurrentThread;
    }
}
