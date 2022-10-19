using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Sync;

/// <summary>
///     An <see cref="IServerMessenger" /> that works by storing messages in the database.
/// </summary>
public abstract class DatabaseServerMessenger : ServerMessengerBase, IDisposable
{
    private readonly CacheRefresherCollection _cacheRefreshers;
    private readonly CancellationToken _cancellationToken;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly Lazy<SyncBootState?> _initialized;
    private readonly LastSyncedFileManager _lastSyncedFileManager;

    private readonly object _locko = new();
    /*
     * this messenger writes ALL instructions to the database,
     * but only processes instructions coming from remote servers,
     *  thus ensuring that instructions run only once
     */

    private readonly IMainDom _mainDom;
    private readonly IServerRoleAccessor _serverRoleAccessor;
    private readonly ISyncBootStateAccessor _syncBootStateAccessor;
    private readonly ManualResetEvent _syncIdle;
    private bool _disposedValue;
    private DateTime _lastPruned;
    private DateTime _lastSync;
    private bool _syncing;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseServerMessenger" /> class.
    /// </summary>
    protected DatabaseServerMessenger(
        IMainDom mainDom,
        CacheRefresherCollection cacheRefreshers,
        IServerRoleAccessor serverRoleAccessor,
        ILogger<DatabaseServerMessenger> logger,
        bool distributedEnabled,
        ISyncBootStateAccessor syncBootStateAccessor,
        IHostingEnvironment hostingEnvironment,
        ICacheInstructionService cacheInstructionService,
        IJsonSerializer jsonSerializer,
        LastSyncedFileManager lastSyncedFileManager,
        IOptionsMonitor<GlobalSettings> globalSettings)
        : base(distributedEnabled)
    {
        _cancellationToken = _cancellationTokenSource.Token;
        _mainDom = mainDom;
        _cacheRefreshers = cacheRefreshers;
        _serverRoleAccessor = serverRoleAccessor;
        _hostingEnvironment = hostingEnvironment;
        Logger = logger;
        _syncBootStateAccessor = syncBootStateAccessor;
        CacheInstructionService = cacheInstructionService;
        JsonSerializer = jsonSerializer;
        _lastSyncedFileManager = lastSyncedFileManager;
        GlobalSettings = globalSettings.CurrentValue;
        _lastPruned = _lastSync = DateTime.UtcNow;
        _syncIdle = new ManualResetEvent(true);

        globalSettings.OnChange(x => GlobalSettings = x);
        using (var process = Process.GetCurrentProcess())
        {
            // See notes on _localIdentity
            LocalIdentity = Environment.MachineName // eg DOMAIN\SERVER
                            + "/" + hostingEnvironment.ApplicationId // eg /LM/S3SVC/11/ROOT
                            + " [P" + process.Id // eg 1234
                            + "/D" + AppDomain.CurrentDomain.Id // eg 22
                            + "] " + Guid.NewGuid().ToString("N").ToUpper(); // make it truly unique
        }

        _initialized = new Lazy<SyncBootState?>(InitializeWithMainDom);
    }

    public GlobalSettings GlobalSettings { get; private set; }

    protected ILogger<DatabaseServerMessenger> Logger { get; }

    protected ICacheInstructionService CacheInstructionService { get; }

    protected IJsonSerializer JsonSerializer { get; }

    /// <summary>
    ///     Gets the unique local identity of the executing AppDomain.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         It is not only about the "server" (machine name and appDomainappId), but also about
    ///         an AppDomain, within a Process, on that server - because two AppDomains running at the same
    ///         time on the same server (eg during a restart) are, practically, a LB setup.
    ///     </para>
    ///     <para>
    ///         Practically, all we really need is the guid, the other infos are here for information
    ///         and debugging purposes.
    ///     </para>
    /// </remarks>
    protected string LocalIdentity { get; }

    /// <summary>
    ///     Synchronize the server (throttled).
    /// </summary>
    public override void Sync()
    {
        if (!EnsureInitialized())
        {
            return;
        }

        lock (_locko)
        {
            if (_syncing)
            {
                return;
            }

            // Don't continue if we are released
            if (_cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (DateTime.UtcNow - _lastSync <= GlobalSettings.DatabaseServerMessenger.TimeBetweenSyncOperations)
            {
                return;
            }

            // Set our flag and the lock to be in it's original state (i.e. it can be awaited)
            _syncing = true;
            _syncIdle.Reset();
            _lastSync = DateTime.UtcNow;
        }

        try
        {
            ProcessInstructionsResult result = CacheInstructionService.ProcessInstructions(
                _cacheRefreshers,
                _serverRoleAccessor.CurrentServerRole,
                _cancellationToken,
                LocalIdentity,
                _lastPruned,
                _lastSyncedFileManager.LastSyncedId);

            if (result.InstructionsWerePruned)
            {
                _lastPruned = _lastSync;
            }

            if (result.LastId > 0)
            {
                _lastSyncedFileManager.SaveLastSyncedId(result.LastId);
            }
        }
        finally
        {
            lock (_locko)
            {
                // We must reset our flag and signal any waiting locks
                _syncing = false;
            }

            _syncIdle.Set();
        }
    }

    /// <summary>
    ///     Dispose
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Returns true if initialization was successfull (i.e. Is MainDom)
    /// </summary>
    protected bool EnsureInitialized() => _initialized.Value.HasValue;

    #region Messenger

    // we don't care if there are servers listed or not,
    // if distributed call is enabled we will make the call
    protected override bool RequiresDistributed(ICacheRefresher refresher, MessageType dispatchType)
        => EnsureInitialized() && DistributedEnabled;

    protected override void DeliverRemote(
        ICacheRefresher refresher,
        MessageType messageType,
        IEnumerable<object>? ids = null,
        string? json = null)
    {
        var idsA = ids?.ToArray();

        if (GetArrayType(idsA, out Type? idType) == false)
        {
            throw new ArgumentException("All items must be of the same type, either int or Guid.", nameof(ids));
        }

        IEnumerable<RefreshInstruction> instructions =
            RefreshInstruction.GetInstructions(refresher, JsonSerializer, messageType, idsA, idType, json);

        CacheInstructionService.DeliverInstructions(instructions, LocalIdentity);
    }

    #endregion

    #region Sync

    /// <summary>
    ///     Boots the messenger.
    /// </summary>
    private SyncBootState? InitializeWithMainDom()
    {
        // weight:10, must release *before* the published snapshot service, because once released
        // the service will *not* be able to properly handle our notifications anymore.
        const int weight = 10;

        var registered = _mainDom.Register(
            release: () =>
            {
                lock (_locko)
                {
                    _cancellationTokenSource.Cancel(); // no more syncs
                }

                // Wait a max of 3 seconds and then return, so that we don't block
                // the entire MainDom callbacks chain and prevent the AppDomain from
                // properly releasing MainDom - a timeout here means that one refresher
                // is taking too much time processing, however when it's done we will
                // not update lastId and stop everything.
                var idle = _syncIdle.WaitOne(3000);
                if (idle == false)
                {
                    Logger.LogWarning(
                        "The wait lock timed out, application is shutting down. The current instruction batch will be re-processed.");
                }
            },
            weight: weight);

        if (registered == false)
        {
            // return null if we cannot initialize
            return null;
        }

        return InitializeColdBootState();
    }

    /// <summary>
    /// Initializes a server that has never synchronized before.
    /// </summary>
    /// <remarks>
    /// Thread safety: this is NOT thread safe. Because it is NOT meant to run multi-threaded.
    /// Callers MUST ensure thread-safety.
    /// </remarks>
    private SyncBootState InitializeColdBootState()
    {
        lock (_locko)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                return SyncBootState.Unknown;
            }

            SyncBootState syncState = _syncBootStateAccessor.GetSyncBootState();

            if (syncState == SyncBootState.ColdBoot)
            {
                // Get the last id in the db and store it.
                // Note: Do it BEFORE initializing otherwise some instructions might get lost
                // when doing it before. Some instructions might run twice but this is not an issue.
                var maxId = CacheInstructionService.GetMaxInstructionId();

                // if there is a max currently, or if we've never synced
                if (maxId > 0 || _lastSyncedFileManager.LastSyncedId < 0)
                {
                    _lastSyncedFileManager.SaveLastSyncedId(maxId);
                }
            }

            return syncState;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _syncIdle.Dispose();
            }

            _disposedValue = true;
        }
    }

    #endregion
}
