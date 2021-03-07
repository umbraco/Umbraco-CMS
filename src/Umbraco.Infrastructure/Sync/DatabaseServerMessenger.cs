using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Sync
{
    /// <summary>
    /// An <see cref="IServerMessenger"/> that works by storing messages in the database.
    /// </summary>
    public abstract class DatabaseServerMessenger : ServerMessengerBase
    {
        /*
         * this messenger writes ALL instructions to the database,
         * but only processes instructions coming from remote servers,
         *  thus ensuring that instructions run only once
         */

        private readonly IMainDom _mainDom;
        private readonly ManualResetEvent _syncIdle;
        private readonly object _locko = new object();
        private readonly IHostingEnvironment _hostingEnvironment;

        private readonly Lazy<string> _distCacheFilePath;
        private int _lastId = -1;
        private DateTime _lastSync;
        private DateTime _lastPruned;
        private readonly Lazy<bool> _initialized;
        private bool _syncing;
        private bool _released;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseServerMessenger"/> class.
        /// </summary>
        protected DatabaseServerMessenger(
            IMainDom mainDom,
            ILogger<DatabaseServerMessenger> logger,
            bool distributedEnabled,
            DatabaseServerMessengerCallbacks callbacks,
            IHostingEnvironment hostingEnvironment,
            ICacheInstructionService cacheInstructionService,
            IJsonSerializer jsonSerializer,
            IOptions<GlobalSettings> globalSettings)
            : base(distributedEnabled)
        {
            _mainDom = mainDom;
            _hostingEnvironment = hostingEnvironment;
            Logger = logger;
            Callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
            CacheInstructionService = cacheInstructionService;
            JsonSerializer = jsonSerializer;
            GlobalSettings = globalSettings.Value;
            _lastPruned = _lastSync = DateTime.UtcNow;
            _syncIdle = new ManualResetEvent(true);
            _distCacheFilePath = new Lazy<string>(() => GetDistCacheFilePath(hostingEnvironment));

            // See notes on _localIdentity
            LocalIdentity = NetworkHelper.MachineName // eg DOMAIN\SERVER
                + "/" + hostingEnvironment.ApplicationId // eg /LM/S3SVC/11/ROOT
                + " [P" + Process.GetCurrentProcess().Id // eg 1234
                + "/D" + AppDomain.CurrentDomain.Id // eg 22
                + "] " + Guid.NewGuid().ToString("N").ToUpper(); // make it truly unique

            _initialized = new Lazy<bool>(EnsureInitialized);
        }

        private string DistCacheFilePath => _distCacheFilePath.Value;

        public DatabaseServerMessengerCallbacks Callbacks { get; }

        public GlobalSettings GlobalSettings { get; }

        protected ILogger<DatabaseServerMessenger> Logger { get; }

        protected ICacheInstructionService CacheInstructionService { get; }

        protected IJsonSerializer JsonSerializer { get; }

        /// <summary>
        /// Gets the unique local identity of the executing AppDomain.
        /// </summary>
        /// <remarks>
        /// <para>It is not only about the "server" (machine name and appDomainappId), but also about
        /// an AppDomain, within a Process, on that server - because two AppDomains running at the same
        /// time on the same server (eg during a restart) are, practically, a LB setup.</para>
        /// <para>Practically, all we really need is the guid, the other infos are here for information
        /// and debugging purposes.</para>
        /// </remarks>
        protected string LocalIdentity { get; }

        #region Messenger

        // we don't care if there are servers listed or not,
        // if distributed call is enabled we will make the call
        protected override bool RequiresDistributed(ICacheRefresher refresher, MessageType dispatchType)
            => _initialized.Value && DistributedEnabled;

        protected override void DeliverRemote(
            ICacheRefresher refresher,
            MessageType messageType,
            IEnumerable<object> ids = null,
            string json = null)
        {
            var idsA = ids?.ToArray();

            if (GetArrayType(idsA, out Type idType) == false)
            {
                throw new ArgumentException("All items must be of the same type, either int or Guid.", nameof(ids));
            }

            IEnumerable<RefreshInstruction> instructions = RefreshInstruction.GetInstructions(refresher, JsonSerializer, messageType, idsA, idType, json);

            CacheInstructionService.DeliverInstructions(instructions, LocalIdentity);
        }

        #endregion

        #region Sync

        /// <summary>
        /// Boots the messenger.
        /// </summary>
        private bool EnsureInitialized()
        {
            // weight:10, must release *before* the published snapshot service, because once released
            // the service will *not* be able to properly handle our notifications anymore.
            const int weight = 10;

            var registered = _mainDom.Register(
                release: () =>
                {
                    lock (_locko)
                    {
                        _released = true; // no more syncs
                    }

                    // Wait a max of 5 seconds and then return, so that we don't block
                    // the entire MainDom callbacks chain and prevent the AppDomain from
                    // properly releasing MainDom - a timeout here means that one refresher
                    // is taking too much time processing, however when it's done we will
                    // not update lastId and stop everything.
                    var idle = _syncIdle.WaitOne(5000);
                    if (idle == false)
                    {
                        Logger.LogWarning("The wait lock timed out, application is shutting down. The current instruction batch will be re-processed.");
                    }
                },
                weight: weight);

            if (registered == false)
            {
                return false;
            }

            ReadLastSynced(); // get _lastId

            CacheInstructionServiceInitializationResult result = CacheInstructionService.EnsureInitialized(_released, _lastId);

            if (result.ColdBootRequired)
            {
                // If there is a max currently, or if we've never synced.
                if (result.MaxId > 0 || result.LastId < 0)
                {
                    SaveLastSynced(result.MaxId);
                }

                // Execute initializing callbacks.
                if (Callbacks.InitializingCallbacks != null)
                {
                    foreach (Action callback in Callbacks.InitializingCallbacks)
                    {
                        callback();
                    }
                }
            }

            return result.Initialized;
        }        

        /// <summary>
        /// Synchronize the server (throttled).
        /// </summary>
        public override void Sync()
        {
            if (!_initialized.Value)
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
                if (_released)
                {
                    return;
                }

                if ((DateTime.UtcNow - _lastSync) <= GlobalSettings.DatabaseServerMessenger.TimeBetweenSyncOperations)
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
                CacheInstructionServiceProcessInstructionsResult result = CacheInstructionService.ProcessInstructions(_released, LocalIdentity, _lastPruned);
                if (result.InstructionsWerePruned)
                {
                    _lastPruned = _lastSync;
                }

                if (result.LastId > 0)
                {
                    SaveLastSynced(result.LastId);
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
        /// Reads the last-synced id from file into memory.
        /// </summary>
        /// <remarks>
        /// Thread safety: this is NOT thread safe. Because it is NOT meant to run multi-threaded.
        /// </remarks>
        private void ReadLastSynced()
        {
            if (File.Exists(DistCacheFilePath) == false)
            {
                return;
            }

            var content = File.ReadAllText(DistCacheFilePath);
            if (int.TryParse(content, out var last))
            {
                _lastId = last;
            }
        }

        /// <summary>
        /// Updates the in-memory last-synced id and persists it to file.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Thread safety: this is NOT thread safe. Because it is NOT meant to run multi-threaded.
        /// </remarks>
        private void SaveLastSynced(int id)
        {
            File.WriteAllText(DistCacheFilePath, id.ToString(CultureInfo.InvariantCulture));
            _lastId = id;
        }

        private string GetDistCacheFilePath(IHostingEnvironment hostingEnvironment)
        {
            var fileName = _hostingEnvironment.ApplicationId.ReplaceNonAlphanumericChars(string.Empty) + "-lastsynced.txt";

            var distCacheFilePath = Path.Combine(hostingEnvironment.LocalTempPath, "DistCache", fileName);

            //ensure the folder exists
            var folder = Path.GetDirectoryName(distCacheFilePath);
            if (folder == null)
            {
                throw new InvalidOperationException("The folder could not be determined for the file " + distCacheFilePath);
            }

            if (Directory.Exists(folder) == false)
            {
                Directory.CreateDirectory(folder);
            }

            return distCacheFilePath;
        }

        #endregion
    }
}
