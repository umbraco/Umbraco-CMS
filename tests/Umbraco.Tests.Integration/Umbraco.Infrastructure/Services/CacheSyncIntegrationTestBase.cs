// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Shared harness for cache sync tests under <c>LoadBalanceIsolatedCaches()</c>: real app caches, a recording
///     cache refresher, and helpers that act as another server would by writing cache versions and instructions
///     straight to the database.
/// </summary>
internal abstract class CacheSyncIntegrationTestBase : UmbracoIntegrationTestWithContent
{
    protected const string RemoteIdentity = "remote-server";

    protected static readonly string ContentCacheKey = typeof(IContent).FullName!;

    protected ICacheSyncService CacheSyncService => GetRequiredService<ICacheSyncService>();

    protected ICacheInstructionService CacheInstructionService => GetRequiredService<ICacheInstructionService>();

    protected IRepositoryCacheVersionRepository CacheVersionRepository => GetRequiredService<IRepositoryCacheVersionRepository>();

    protected ILastSyncedRepository LastSyncedRepository => GetRequiredService<ILastSyncedRepository>();

    protected ILastSyncedManager LastSyncedManager => GetRequiredService<ILastSyncedManager>();

    protected RefreshRecorder Refreshes => GetRequiredService<RefreshRecorder>();

    protected string LocalIdentity => GetRequiredService<IMachineInfoFactory>().GetLocalIdentity();

    // Repositories only use (and sync) their cache policies with real app caches; the test host defaults to NoCache.
    // The request cache is a mock so the version is not cached across the whole test.
    protected override void ConfigureTestServices(IServiceCollection services)
        => services.AddSingleton(AppCaches.Create(Mock.Of<IRequestCache>()));

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.LoadBalanceIsolatedCaches();

        builder.Services.AddSingleton<RefreshRecorder>();
        builder.CacheRefreshers().Add<RecordingCacheRefresher>();
    }

    [SetUp]
    public void ResetRefreshes() => Refreshes.Reset();

    // Dedicated threads: the callers block on synchronous waits, which starves the pool when many fixtures run together.
    protected static Task RunDetached(Action action)
    {
        using (ExecutionContext.SuppressFlow())
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }

    // Identical instructions are processed once per run, so give each one a distinct payload where several are needed.
    protected static RefreshInstruction RecordingInstruction(int marker = 0)
        => new(RecordingCacheRefresher.UniqueId, RefreshMethodType.RefreshByJson, Guid.Empty, 0, null!, $"[{marker}]");

    /// <summary>Writes a cache version the way another server would: to the database only.</summary>
    protected void WriteRemoteCacheVersion(string cacheKey)
    {
        using IScope scope = ScopeProvider.CreateScope();
        CacheVersionRepository.SaveAsync(new RepositoryCacheVersion
        {
            Identifier = cacheKey,
            Version = Guid.NewGuid().ToString(),
        }).GetAwaiter().GetResult();
        scope.Complete();
    }

    /// <summary>
    ///     Makes this server aware of the current IContent cache version and populates the isolated cache with Textpage.
    ///     Creating content does not write a version row, and without one a read adopts whatever remote version it finds
    ///     later instead of syncing.
    /// </summary>
    protected void InitialiseLocalContentCacheVersion()
    {
        WriteRemoteCacheVersion(ContentCacheKey);
        ContentService.GetById(Textpage.Key);
    }

    protected void DeliverRemoteInstructions(params RefreshInstruction[] instructions)
        => CacheInstructionService.DeliverInstructions(instructions, RemoteIdentity);

    protected RefreshInstruction ContentInstruction(IContent content, TreeChangeTypes changeTypes)
        => new(
            ContentCacheRefresher.UniqueId,
            RefreshMethodType.RefreshByJson,
            Guid.Empty,
            0,
            null!,
            GetRequiredService<ContentCacheRefresher>().Serialize(new ContentCacheRefresher.JsonPayload
            {
                Id = content.Id,
                Key = content.Key,
                ChangeTypes = changeTypes,
            }));

    protected RefreshInstruction ContentRefreshNodeInstruction(IContent content)
        => ContentInstruction(content, TreeChangeTypes.RefreshNode);

    protected IContent? CachedContent(Guid key)
        => AppCaches.IsolatedCaches.GetOrCreate<IContent>().GetCacheItem<IContent>(RepositoryCacheKeys.GetGuidKey<IContent>(key));

    protected int? LastSyncedInternalIdInDatabase()
    {
        using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
        return LastSyncedRepository.GetInternalIdAsync().GetAwaiter().GetResult();
    }

    internal sealed class RefreshRecorder
    {
        private int _refresh;
        private int _refreshInternal;

        public int RefreshCount => Volatile.Read(ref _refresh);

        public int RefreshInternalCount => Volatile.Read(ref _refreshInternal);

        public void Refreshed() => Interlocked.Increment(ref _refresh);

        public void RefreshedInternal() => Interlocked.Increment(ref _refreshInternal);

        public void Reset()
        {
            Interlocked.Exchange(ref _refresh, 0);
            Interlocked.Exchange(ref _refreshInternal, 0);
        }
    }

    // Private so the type scanner does not register it for fixtures that do not register its recorder.
    private sealed class RecordingCacheRefresher(RefreshRecorder recorder) : IJsonCacheRefresher
    {
        public static readonly Guid UniqueId = new("6D3F2D1E-4C0B-4C7A-9B1E-6F0E1F2A3B4C");

        public Guid RefresherUniqueId => UniqueId;

        public string Name => "Recording cache refresher";

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

        public void Refresh(string json) => recorder.Refreshed();

        public void RefreshInternal(string json) => recorder.RefreshedInternal();
    }
}
