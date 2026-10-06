// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.DistributedLocking;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     With <c>LoadBalanceIsolatedCaches()</c>, a repository read that finds its isolated cache out of date syncs inline
///     (<see cref="ICacheSyncService.SyncInternal" />) inside the caller's scope, which may hold or have queued distributed
///     locks such as the ContentTree write lock of a publish. The full sync (<see cref="ICacheSyncService.SyncAll" />) holds
///     <c>CacheInstructionService._syncLock</c> while its refreshers take those same locks. These tests pin the invariant
///     that the inline sync never waits for that lock, never takes distributed locks of its own, and still delivers
///     fresh isolated caches.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class CacheSyncServiceLockOrderingTests : CacheSyncIntegrationTestBase
{
    private static readonly FieldInfo _syncLockField =
        typeof(CacheInstructionService).GetField("_syncLock", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("CacheInstructionService._syncLock not found; update the test.");

    private LockRecorder Recorder => GetRequiredService<LockRecorder>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.Services.AddSingleton<LockRecorder>();

        builder.Services.AddUnique<IDistributedLockingMechanismFactory>(sp => new RecordingLockingMechanismFactory(
            ActivatorUtilities.CreateInstance<DefaultDistributedLockingMechanismFactory>(sp),
            sp.GetRequiredService<LockRecorder>(),
            sp));

        builder.Services.AddUnique<ICacheSyncService>(sp => new RecordingCacheSyncService(
            ActivatorUtilities.CreateInstance<Cms.Core.CacheSyncService>(sp),
            sp.GetRequiredService<LockRecorder>()));
    }

    [SetUp]
    public void ResetLockRecorder() => Recorder.Clear();

    [Test]
    public void SyncInternal_Completes_While_Another_Thread_Holds_The_Full_Sync_Lock()
    {
        using var holder = HoldSyncLockOnAnotherThread();

        // A save or publish that already holds the ContentTree write lock finds its cache stale and syncs inline.
        Task caller = RunDetached(() =>
        {
            using IScope scope = ScopeProvider.CreateScope();
            scope.EagerWriteLock(Constants.Locks.ContentTree);
            CacheSyncService.SyncInternal(CancellationToken.None);
            scope.Complete();
        });

        var completed = caller.Wait(TimeSpan.FromSeconds(5));
        holder.Dispose();
        caller.Wait();

        Assert.That(
            completed,
            Is.True,
            "SyncInternal waited for the full sync lock while the caller held ContentTree. If the thread holding that lock is waiting for ContentTree, that is a deadlock SQL Server cannot detect.");
    }

    [Test]
    public void SyncInternal_Takes_No_Distributed_Locks_Of_Its_Own()
    {
        InitialiseLocalContentCacheVersion();
        DeliverRemoteInstructions(ContentRefreshNodeInstruction(Textpage), DomainRefreshAllInstruction(), RecordingInstruction());
        Recorder.Clear();

        using (IScope scope = ScopeProvider.CreateScope())
        {
            // Queued only: acquired on this scope's first database access.
            scope.WriteLock(Constants.Locks.ContentTree);

            CacheSyncService.SyncInternal(CancellationToken.None);

            scope.Complete();
        }

        Assume.That(Refreshes.RefreshInternalCount, Is.GreaterThan(0), "The remote instructions were not processed, so this test proves nothing.");

        Assert.Multiple(() =>
        {
            Assert.That(
                Recorder.Obtained.Where(x => x.Type == DistributedLockType.ReadLock),
                Is.Empty,
                "The inline sync took a read lock of its own.");
            Assert.That(
                Recorder.Obtained.Where(x => x.LockId != Constants.Locks.ContentTree),
                Is.Empty,
                "The inline sync took a lock other than the caller's own.");
            Assert.That(
                Recorder.ObtainedUnderSyncLock(),
                Is.Empty,
                "A distributed lock was obtained while the full sync lock was held by the inline sync.");
        });
    }

    [Test]
    public void SyncInternal_Runs_Only_RefreshInternal_And_SyncAll_Runs_Both()
    {
        DeliverRemoteInstructions(RecordingInstruction());

        CacheSyncService.SyncInternal(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(Refreshes.RefreshInternalCount, Is.EqualTo(1));
            Assert.That(Refreshes.RefreshCount, Is.EqualTo(0), "SyncInternal ran the published-cache refresh.");
        });

        // The external last-synced id is separate, so the full sync processes the same instruction again.
        CacheSyncService.SyncAll(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(Refreshes.RefreshInternalCount, Is.EqualTo(2));
            Assert.That(Refreshes.RefreshCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void Read_With_A_Stale_Cache_Sees_Content_Saved_On_Another_Server_While_A_Full_Sync_Is_In_Progress()
    {
        const string newName = "Renamed on another server";

        InitialiseLocalContentCacheVersion();
        Assume.That(CachedContent(Textpage.Key)?.Name, Is.EqualTo(Textpage.Name), "Textpage was not cached, so this test proves nothing.");

        SimulateRemoteSave(Textpage.Key, newName);
        Assume.That(CachedContent(Textpage.Key)?.Name, Is.EqualTo(Textpage.Name), "The remote save touched this server's isolated cache, so this test proves nothing.");

        string? observedName = null;
        bool completed;
        using (var holder = HoldSyncLockOnAnotherThread())
        {
            // A publish on this server: it queues the ContentTree write lock and then reads the document.
            Task reader = RunDetached(() =>
            {
                using IScope scope = ScopeProvider.CreateScope();
                scope.WriteLock(Constants.Locks.ContentTree);
                observedName = ContentService.GetById(Textpage.Key)?.Name;
                scope.Complete();
            });

            completed = reader.Wait(TimeSpan.FromSeconds(10));
            holder.Dispose();
            reader.Wait();
        }

        Assume.That(Recorder.InlineSyncs, Is.GreaterThan(0), "The read did not trigger an inline cache sync, so this test proves nothing.");

        Assert.Multiple(() =>
        {
            Assert.That(completed, Is.True, "The read waited for the full sync lock while holding ContentTree.");
            Assert.That(observedName, Is.EqualTo(newName), "The read was served from the stale isolated cache.");
        });
    }

    [Test]
    public void SyncInternal_Clears_The_Id_Key_Map_For_Content_Deleted_On_Another_Server()
    {
        InitialiseLocalContentCacheVersion();
        Assume.That(IdKeyMap.GetIdForKey(Subpage.Key, UmbracoObjectTypes.Document).Success, Is.True);

        SimulateRemoteDelete(Subpage.Key);
        Assume.That(
            IdKeyMap.GetIdForKey(Subpage.Key, UmbracoObjectTypes.Document).Success,
            Is.True,
            "The remote delete touched this server's id/key map, so this test proves nothing.");

        ContentService.GetById(Textpage.Key);

        Assume.That(Recorder.InlineSyncs, Is.GreaterThan(0), "The read did not trigger an inline cache sync, so this test proves nothing.");
        Assert.That(
            IdKeyMap.GetIdForKey(Subpage.Key, UmbracoObjectTypes.Document).Success,
            Is.False,
            "The id/key map still resolves content deleted on another server.");
    }

    [Test]
    public void Concurrent_SyncInternal_Calls_Do_Not_Throw_And_Converge()
    {
        InitialiseLocalContentCacheVersion();
        DeliverRemoteInstructions(ContentRefreshNodeInstruction(Textpage), RecordingInstruction());
        Assume.That(CachedContent(Textpage.Key), Is.Not.Null, "Textpage was not cached, so this test proves nothing.");
        Assume.That(LastSyncedInternalIdInDatabase(), Is.Null, "A last-synced row already exists, so this test proves nothing.");
        const int callers = 8;
        using var start = new ManualResetEventSlim();
        var exceptions = new ConcurrentQueue<Exception>();
        Task[] tasks = Enumerable.Range(0, callers).Select(_ => RunDetached(() =>
        {
            start.Wait();
            try
            {
                using IScope scope = ScopeProvider.CreateScope();
                CacheSyncService.SyncInternal(CancellationToken.None);
                scope.Complete();
            }
            catch (Exception ex)
            {
                exceptions.Enqueue(ex);
            }
        })).ToArray();

        start.Set();
        Assert.That(Task.WaitAll(tasks, TimeSpan.FromSeconds(60)), Is.True, "Concurrent inline syncs did not finish.");

        ProcessInstructionsResult afterConvergence = CacheInstructionService.ProcessInternalInstructions(
            GetRequiredService<CacheRefresherCollection>(),
            CancellationToken.None,
            LocalIdentity);

        Assert.Multiple(() =>
        {
            Assert.That(exceptions, Is.Empty, string.Join("; ", exceptions.Select(x => x.ToString())));
            Assert.That(CachedContent(Textpage.Key), Is.Null, "The isolated cache entry was not cleared.");
            Assert.That(Refreshes.RefreshCount, Is.EqualTo(0), "An inline sync ran the published-cache refresh.");
            Assert.That(Refreshes.RefreshInternalCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(LastSyncedInternalIdInDatabase(), Is.Null, "An inline sync persisted its checkpoint.");
            Assert.That(afterConvergence.NumberOfInstructionsProcessed, Is.EqualTo(0), "Already processed instructions were processed again.");
        });
    }

    [Test]
    public void SyncInternal_Clears_Data_Type_Configuration_Changed_On_Another_Server()
    {
        const int newMaxChars = 123;
        Guid dataTypeKey = Constants.DataTypes.Guids.TextstringGuid;
        var dataTypeCacheKey = typeof(IDataType).FullName!;
        IDataTypeConfigurationCache configurationCache = GetRequiredService<IDataTypeConfigurationCache>();
        IDataTypeService dataTypeService = GetRequiredService<IDataTypeService>();

        // This server has seen the current IDataType cache version and holds the configuration in its cache.
        WriteRemoteCacheVersion(dataTypeCacheKey);
        TextboxConfiguration? before = configurationCache.GetConfigurationAs<TextboxConfiguration>(dataTypeKey);
        Assume.That(before, Is.Not.Null);
        Assume.That(before!.MaxChars, Is.Not.EqualTo(newMaxChars));

        // Another server changes the configuration: the write bypasses this server's caches, then its version and instruction arrive.
        IDataType dataType;
        using (IScope scope = ScopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.None))
        {
            dataType = dataTypeService.GetAsync(dataTypeKey).GetAwaiter().GetResult()!;
            dataType.ConfigurationData["maxChars"] = newMaxChars;
            Attempt<IDataType, DataTypeOperationStatus> updated = dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey).GetAwaiter().GetResult();
            Assume.That(updated.Success, Is.True, $"The remote change could not be made: {updated.Status}.");
            scope.Complete();
        }

        WriteRemoteCacheVersion(dataTypeCacheKey);
        DeliverRemoteInstructions(new RefreshInstruction(
            DataTypeCacheRefresher.UniqueId,
            RefreshMethodType.RefreshByJson,
            Guid.Empty,
            0,
            null!,
            GetRequiredService<DataTypeCacheRefresher>().Serialize(new DataTypeCacheRefresher.JsonPayload(dataType.Id, dataTypeKey, false))));
        Assume.That(configurationCache.GetConfigurationAs<TextboxConfiguration>(dataTypeKey)!.MaxChars, Is.Not.EqualTo(newMaxChars), "The remote change touched this server's configuration cache, so this test proves nothing.");

        CacheSyncService.SyncInternal(CancellationToken.None);

        Assert.That(configurationCache.GetConfigurationAs<TextboxConfiguration>(dataTypeKey)!.MaxChars, Is.EqualTo(newMaxChars), "The data type configuration cache still holds the configuration from before the remote change.");
    }

    [Test]
    public void SyncInternal_Keeps_Its_Checkpoint_When_The_Periodic_Sync_Records_A_Lower_One()
    {
        DeliverRemoteInstructions(RecordingInstruction(1));
        var firstRowId = CacheInstructionService.GetMaxInstructionId();
        DeliverRemoteInstructions(RecordingInstruction(2));
        DeliverRemoteInstructions(RecordingInstruction(3));
        CacheSyncService.SyncInternal(CancellationToken.None);
        Assume.That(Refreshes.RefreshInternalCount, Is.EqualTo(3));

        // A full sync that started from an older external id records the lower id it reached.
        LastSyncedManager.SaveLastSyncedExternalAsync(firstRowId).GetAwaiter().GetResult();
        LastSyncedManager.SaveLastSyncedInternalAsync(firstRowId).GetAwaiter().GetResult();
        DeliverRemoteInstructions(RecordingInstruction(4));

        CacheSyncService.SyncInternal(CancellationToken.None);

        Assert.That(Refreshes.RefreshInternalCount, Is.EqualTo(4), "Already processed instructions were processed again.");
    }

    [Test]
    public void SyncInternal_Writes_Nothing_To_The_Database()
    {
        var persistedBefore = LastSyncedInternalIdInDatabase();
        DeliverRemoteInstructions(RecordingInstruction(1), RecordingInstruction(2));

        using (IScope scope = ScopeProvider.CreateScope())
        {
            CacheSyncService.SyncInternal(CancellationToken.None);
            scope.Complete();
        }

        Assume.That(Refreshes.RefreshInternalCount, Is.EqualTo(2), "The instructions were not processed, so this test proves nothing.");

        CacheSyncService.SyncInternal(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(LastSyncedInternalIdInDatabase(), Is.EqualTo(persistedBefore), "The inline sync persisted its checkpoint.");
            Assert.That(Refreshes.RefreshInternalCount, Is.EqualTo(2), "The in-memory checkpoint did not advance.");
        });
    }

    [Test]
    public void SyncInternal_Starts_From_The_Periodic_Checkpoint_After_A_Restart()
    {
        DeliverRemoteInstructions(RecordingInstruction(1));
        var firstRowId = CacheInstructionService.GetMaxInstructionId();
        DeliverRemoteInstructions(RecordingInstruction(2));
        DeliverRemoteInstructions(RecordingInstruction(3));
        CacheSyncService.SyncInternal(CancellationToken.None);
        Assume.That(Refreshes.RefreshInternalCount, Is.EqualTo(3), "The instructions were not processed, so this test proves nothing.");

        // The periodic sync persisted a checkpoint behind the inline one, and a restart loses the in-memory one.
        LastSyncedManager.SaveLastSyncedExternalAsync(firstRowId).GetAwaiter().GetResult();
        ((Cms.Core.Sync.LastSyncedManager)LastSyncedManager).ClearLocalCache();

        CacheSyncService.SyncInternal(CancellationToken.None);

        Assert.That(Refreshes.RefreshInternalCount, Is.EqualTo(5), "Only the instructions after the persisted checkpoint are re-run.");
    }

    [Test]
    public void SyncAll_Is_Not_Blocked_By_An_Inline_Sync_Inside_An_Open_Transaction()
    {
        if (BaseTestDatabase.IsSqlite())
        {
            Assert.Ignore("SQLite allows a single writer, so an open transaction stalls the full sync regardless of what the inline sync does; this scenario is verified on SQL Server.");
        }

        DeliverRemoteInstructions(RecordingInstruction());

        using IScope scope = ScopeProvider.CreateScope();
        CacheSyncService.SyncInternal(CancellationToken.None);

        // The request that ran the inline sync is still in its transaction when the periodic sync persists its checkpoint.
        Task fullSync = RunDetached(() => CacheSyncService.SyncAll(CancellationToken.None));

        Assert.That(fullSync.Wait(TimeSpan.FromSeconds(10)), Is.True, "The full sync waited for the inline sync's transaction to commit.");
        scope.Complete();
    }

    [Test]
    public void SyncInternal_Runs_Safely_Alongside_SyncAll()
    {
        if (BaseTestDatabase.IsSqlite())
        {
            // SQLite allows a single writer: a scope that started reading and then persists the checkpoint cannot
            // become a writer once another writer has committed, so the concurrent callers here stall on the busy
            // timeout instead of contending on a row. Load balancing, and with it the inline sync, is not a SQLite scenario.
            Assert.Ignore("Concurrent checkpoint writers stall on SQLite's single-writer model; this scenario is verified on SQL Server.");
        }

        InitialiseLocalContentCacheVersion();
        DeliverRemoteInstructions(ContentRefreshNodeInstruction(Textpage), RecordingInstruction());

        using var start = new ManualResetEventSlim();
        var exceptions = new ConcurrentQueue<Exception>();

        Task Guarded(Action action) => RunDetached(() =>
        {
            start.Wait();
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exceptions.Enqueue(ex);
            }
        });

        var tasks = new List<Task>
        {
            Guarded(() => CacheSyncService.SyncAll(CancellationToken.None)),
            Guarded(() =>
            {
                using IScope scope = ScopeProvider.CreateScope();
                scope.EagerWriteLock(Constants.Locks.ContentTree);
                CacheSyncService.SyncInternal(CancellationToken.None);
                scope.Complete();
            }),
        };
        tasks.AddRange(Enumerable.Range(0, 3).Select(_ => Guarded(() =>
        {
            using IScope scope = ScopeProvider.CreateScope();
            CacheSyncService.SyncInternal(CancellationToken.None);
            scope.Complete();
        })));

        start.Set();
        Assert.That(Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(60)), Is.True, "Concurrent syncs did not finish.");

        Assert.Multiple(() =>
        {
            Assert.That(exceptions, Is.Empty, string.Join("; ", exceptions.Select(x => x.ToString())));
            Assert.That(Refreshes.RefreshCount, Is.EqualTo(1), "Only the full sync runs the published-cache refresh.");
            Assert.That(Refreshes.RefreshInternalCount, Is.GreaterThanOrEqualTo(1));
        });
    }

    private Lock GetSyncLock()
        => (Lock)_syncLockField.GetValue(CacheInstructionService)!;

    private SyncLockHolder HoldSyncLockOnAnotherThread() => new(GetSyncLock());

    private RefreshInstruction DomainRefreshAllInstruction()
        => new(
            DomainCacheRefresher.UniqueId,
            RefreshMethodType.RefreshByJson,
            Guid.Empty,
            0,
            null!,
            GetRequiredService<DomainCacheRefresher>().Serialize(new DomainCacheRefresher.JsonPayload(0, DomainChangeTypes.RefreshAll)));

    /// <summary>
    ///     Saves as another server would: the write bypasses this server's isolated cache, then the remote server's new
    ///     cache version and its cache instruction arrive.
    /// </summary>
    private void SimulateRemoteSave(Guid key, string newName)
    {
        IContent remote;
        using (IScope scope = ScopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.None))
        {
            remote = ContentService.GetById(key)!;
            remote.Name = newName;
            ContentService.Save(remote);
            scope.Complete();
        }

        WriteRemoteCacheVersion(ContentCacheKey);
        DeliverRemoteInstructions(ContentRefreshNodeInstruction(remote));
    }

    /// <summary>Deletes as another server would, see <see cref="SimulateRemoteSave" />.</summary>
    private void SimulateRemoteDelete(Guid key)
    {
        IContent remote;
        using (IScope scope = ScopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.None))
        {
            remote = ContentService.GetById(key)!;
            ContentService.Delete(remote);
            scope.Complete();
        }

        WriteRemoteCacheVersion(ContentCacheKey);
        DeliverRemoteInstructions(ContentInstruction(remote, TreeChangeTypes.Remove));
    }

    /// <summary>Holds <c>CacheInstructionService._syncLock</c> on another thread until disposed, as the full sync would.</summary>
    private sealed class SyncLockHolder : IDisposable
    {
        private readonly ManualResetEventSlim _release = new();
        private readonly Task _holder;
        private bool _disposed;

        public SyncLockHolder(Lock syncLock)
        {
            using var holding = new ManualResetEventSlim();
            _holder = Task.Factory.StartNew(
                () =>
                {
                    lock (syncLock)
                    {
                        holding.Set();
                        _release.Wait(TimeSpan.FromSeconds(60));
                    }
                },
                TaskCreationOptions.LongRunning);
            holding.Wait();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _release.Set();
            _holder.Wait();
            _release.Dispose();
        }
    }

    /// <summary>Records lock acquisitions and whether <c>CacheInstructionService._syncLock</c> was held.</summary>
    internal sealed class LockRecorder
    {
        private readonly ConcurrentQueue<(int LockId, DistributedLockType Type, bool UnderSyncLock)> _obtained = new();
        private int _inlineSyncs;

        public IReadOnlyList<(int LockId, DistributedLockType Type, bool UnderSyncLock)> Obtained => _obtained.ToList();

        public int InlineSyncs => Volatile.Read(ref _inlineSyncs);

        public void Record(int lockId, DistributedLockType type, bool underSyncLock) => _obtained.Enqueue((lockId, type, underSyncLock));

        public void RecordInlineSync() => Interlocked.Increment(ref _inlineSyncs);

        public void Clear()
        {
            _obtained.Clear();
            Interlocked.Exchange(ref _inlineSyncs, 0);
        }

        public IReadOnlyList<(int LockId, DistributedLockType Type)> ObtainedUnderSyncLock()
            => _obtained.Where(x => x.UnderSyncLock).Select(x => (x.LockId, x.Type)).ToList();
    }

    private sealed class RecordingCacheSyncService(ICacheSyncService inner, LockRecorder recorder) : ICacheSyncService
    {
        public void SyncAll(CancellationToken cancellationToken) => inner.SyncAll(cancellationToken);

        public void SyncInternal(CancellationToken cancellationToken)
        {
            recorder.RecordInlineSync();
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
               && _syncLockField.GetValue(service) is Lock syncLock
               && syncLock.IsHeldByCurrentThread;
    }
}
