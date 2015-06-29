using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using umbraco.interfaces;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// An <see cref="IServerMessenger"/> that works by storing messages in the database.
    /// </summary>
    //
    // abstract because it needs to be inherited by a class that will
    // - trigger Boot() when appropriate
    // - trigger Sync() when appropriate
    //
    // this messenger writes ALL instructions to the database,
    // but only processes instructions coming from remote servers,
    // thus ensuring that instructions run only once
    //
    public abstract class DatabaseServerMessenger : ServerMessengerBase
    {
        private readonly ApplicationContext _appContext;
        private readonly DatabaseServerMessengerOptions _options;
        private readonly object _lock = new object();
        private int _lastId = -1;
        private volatile bool _syncing;
        private DateTime _lastSync;
        private bool _initialized;

        protected ApplicationContext ApplicationContext { get { return _appContext; } }

        protected DatabaseServerMessenger(ApplicationContext appContext, bool distributedEnabled, DatabaseServerMessengerOptions options)
            : base(distributedEnabled)
        {
            if (appContext == null) throw new ArgumentNullException("appContext");
            if (options == null) throw new ArgumentNullException("options");

            _appContext = appContext;
            _options = options;
            _lastSync = DateTime.UtcNow;
        }

        #region Messenger

        protected override bool RequiresDistributed(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, MessageType dispatchType)
        {
            // we don't care if there's servers listed or not, 
            // if distributed call is enabled we will make the call
            return _initialized && DistributedEnabled;
        }

        protected override void DeliverRemote(
            IEnumerable<IServerAddress> servers,
            ICacheRefresher refresher,
            MessageType messageType,
            IEnumerable<object> ids = null,
            string json = null)
        {
            var idsA = ids == null ? null : ids.ToArray();

            Type idType;
            if (GetArrayType(idsA, out idType) == false)
                throw new ArgumentException("All items must be of the same type, either int or Guid.", "ids");

            var instructions = RefreshInstruction.GetInstructions(refresher, messageType, idsA, idType, json);

            var dto = new CacheInstructionDto
            {
                UtcStamp = DateTime.UtcNow,
                Instructions = JsonConvert.SerializeObject(instructions, Formatting.None),
                OriginIdentity = GetLocalIdentity()
            };

            ApplicationContext.DatabaseContext.Database.Insert(dto);
        }

        #endregion

        #region Sync

        /// <summary>
        /// Boots the messenger.
        /// </summary>
        /// <remarks>
        /// Thread safety: this is NOT thread safe. Because it is NOT meant to run multi-threaded.
        /// Callers MUST ensure thread-safety.
        /// </remarks>
        protected void Boot()
        {
            ReadLastSynced();
            Initialize();
        }

        /// <summary>
        /// Initializes a server that has never synchronized before.
        /// </summary>
        /// <remarks>
        /// Thread safety: this is NOT thread safe. Because it is NOT meant to run multi-threaded.
        /// Callers MUST ensure thread-safety.
        /// </remarks>
        private void Initialize()
        {
            if (_lastId < 0) // never synced before
            {
                // we haven't synced - in this case we aren't going to sync the whole thing, we will assume this is a new 
                // server and it will need to rebuild it's own caches, eg Lucene or the xml cache file.
                LogHelper.Warn<DatabaseServerMessenger>("No last synced Id found, this generally means this is a new server/install. The server will rebuild its caches and indexes and then adjust it's last synced id to the latest found in the database and will start maintaining cache updates based on that id");

                // go get the last id in the db and store it
                // note: do it BEFORE initializing otherwise some instructions might get lost
                // when doing it before, some instructions might run twice - not an issue
                var lastId = _appContext.DatabaseContext.Database.ExecuteScalar<int>("SELECT MAX(id) FROM umbracoCacheInstruction");
                if (lastId > 0)
                    SaveLastSynced(lastId);

                // execute initializing callbacks
                if (_options.InitializingCallbacks != null)
                    foreach (var callback in _options.InitializingCallbacks)
                        callback();
            }

            _initialized = true;
        }

        /// <summary>
        /// Synchronize the server (throttled).
        /// </summary>
        protected void Sync()
        {
            if ((DateTime.UtcNow - _lastSync).Seconds <= _options.ThrottleSeconds)
                return;

            if (_syncing) return;

            lock (_lock)
            {
                if (_syncing) return;

                _syncing = true; // lock other threads out
                _lastSync = DateTime.UtcNow;

                using (DisposableTimer.DebugDuration<DatabaseServerMessenger>("Syncing from database..."))
                {
                    ProcessDatabaseInstructions();
                    PruneOldInstructions();
                }

                _syncing = false; // release
            }
        }

        /// <summary>
        /// Process instructions from the database.
        /// </summary>
        /// <remarks>
        /// Thread safety: this is NOT thread safe. Because it is NOT meant to run multi-threaded.
        /// </remarks>
        private void ProcessDatabaseInstructions()
        {
            // NOTE
            // we 'could' recurse to ensure that no remaining instructions are pending in the table before proceeding but I don't think that 
            // would be a good idea since instructions could keep getting added and then all other threads will probably get stuck from serving requests
            // (depending on what the cache refreshers are doing). I think it's best we do the one time check, process them and continue, if there are 
            // pending requests after being processed, they'll just be processed on the next poll.
            //
            // FIXME not true if we're running on a background thread, assuming we can?

            var sql = new Sql().Select("*")
                .From<CacheInstructionDto>()
                .Where<CacheInstructionDto>(dto => dto.Id > _lastId)
                .OrderBy<CacheInstructionDto>(dto => dto.Id);

            var dtos = _appContext.DatabaseContext.Database.Fetch<CacheInstructionDto>(sql);
            if (dtos.Count <= 0) return;

            // only process instructions coming from a remote server, and ignore instructions coming from
            // the local server as they've already been processed. We should NOT assume that the sequence of
            // instructions in the database makes any sense whatsoever, because it's all async.
            var localIdentity = GetLocalIdentity();

            var lastId = 0;
            foreach (var dto in dtos)
            {
                if (dto.OriginIdentity == localIdentity)
                {
                    // just skip that local one but update lastId nevertheless
                    lastId = dto.Id;
                    continue;
                }

                // deserialize remote instructions & skip if it fails
                JArray jsonA;
                try
                {
                    jsonA = JsonConvert.DeserializeObject<JArray>(dto.Instructions);
                }
                catch (JsonException ex)
                {
                    LogHelper.Error<DatabaseServerMessenger>(string.Format("Failed to deserialize instructions ({0}: \"{1}\").", dto.Id, dto.Instructions), ex);
                    lastId = dto.Id; // skip
                    continue;
                }

                // execute remote instructions & update lastId
                try
                {
                    NotifyRefreshers(jsonA);
                    lastId = dto.Id;
                }
                catch (Exception ex)
                {
                    LogHelper.Error<DatabaseServerMessenger>(string.Format("Failed to execute instructions ({0}: \"{1}\").", dto.Id, dto.Instructions), ex);
                    LogHelper.Warn<DatabaseServerMessenger>("BEWARE - DISTRIBUTED CACHE IS NOT UPDATED.");
                    throw;
                 }
            }

            if (lastId > 0)
                SaveLastSynced(lastId);
        }

        /// <summary>
        /// Remove old instructions from the database.
        /// </summary>
        private void PruneOldInstructions()
        {
            _appContext.DatabaseContext.Database.Delete<CacheInstructionDto>("WHERE utcStamp < @pruneDate", 
                new { pruneDate = DateTime.UtcNow.AddDays(-_options.DaysToRetainInstructions) });
        }

        /// <summary>
        /// Reads the last-synced id from file into memory.
        /// </summary>
        /// <remarks>
        /// Thread safety: this is NOT thread safe. Because it is NOT meant to run multi-threaded.
        /// </remarks>
        private void ReadLastSynced()
        {
            var path = SyncFilePath;
            if (File.Exists(path) == false) return;

            var content = File.ReadAllText(path);
            int last;
            if (int.TryParse(content, out last))
                _lastId = last;
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
            File.WriteAllText(SyncFilePath, id.ToString(CultureInfo.InvariantCulture));
            _lastId = id;
        }

        /// <summary>
        /// Gets the local server unique identity.
        /// </summary>
        /// <returns>The unique identity of the local server.</returns>
        protected string GetLocalIdentity()
        {
            return JsonConvert.SerializeObject(new
            {
                machineName = NetworkHelper.MachineName, 
                appDomainAppId = HttpRuntime.AppDomainAppId
            });
        }

        /// <summary>
        /// Gets the sync file path for the local server.
        /// </summary>
        /// <returns>The sync file path for the local server.</returns>
        private static string SyncFilePath
        {
            get
            {
                var tempFolder = IOHelper.MapPath("~/App_Data/TEMP/DistCache/" + NetworkHelper.FileSafeMachineName);
                if (Directory.Exists(tempFolder) == false)
                    Directory.CreateDirectory(tempFolder);

                return Path.Combine(tempFolder, HttpRuntime.AppDomainAppId.ReplaceNonAlphanumericChars(string.Empty) + "-lastsynced.txt");
            }
        }

        #endregion

        #region Notify refreshers

        private static ICacheRefresher GetRefresher(Guid id)
        {
            var refresher = CacheRefreshersResolver.Current.GetById(id);
            if (refresher == null)
                throw new InvalidOperationException("Cache refresher with ID \"" + id + "\" does not exist.");
            return refresher;
        }

        private static IJsonCacheRefresher GetJsonRefresher(Guid id)
        {
            return GetJsonRefresher(GetRefresher(id));
        }

        private static IJsonCacheRefresher GetJsonRefresher(ICacheRefresher refresher)
        {
            var jsonRefresher = refresher as IJsonCacheRefresher;
            if (jsonRefresher == null)
                throw new InvalidOperationException("Cache refresher with ID \"" + refresher.UniqueIdentifier + "\" does not implement " + typeof(IJsonCacheRefresher) + ".");
            return jsonRefresher;
        }

        private static void NotifyRefreshers(IEnumerable<JToken> jsonArray)
        {
            foreach (var jsonItem in jsonArray)
            {
                // could be a JObject in which case we can convert to a RefreshInstruction,
                // otherwise it could be another JArray - in which case we'll iterate that.
                var jsonObj = jsonItem as JObject;
                if (jsonObj != null)
                {
                    var instruction = jsonObj.ToObject<RefreshInstruction>();
                    switch (instruction.RefreshType)
                    {
                        case RefreshMethodType.RefreshAll:
                            RefreshAll(instruction.RefresherId);
                            break;
                        case RefreshMethodType.RefreshByGuid:
                            RefreshByGuid(instruction.RefresherId, instruction.GuidId);
                            break;
                        case RefreshMethodType.RefreshById:
                            RefreshById(instruction.RefresherId, instruction.IntId);
                            break;
                        case RefreshMethodType.RefreshByIds:
                            RefreshByIds(instruction.RefresherId, instruction.JsonIds);
                            break;
                        case RefreshMethodType.RefreshByJson:
                            RefreshByJson(instruction.RefresherId, instruction.JsonPayload);
                            break;
                        case RefreshMethodType.RemoveById:
                            RemoveById(instruction.RefresherId, instruction.IntId);
                            break;
                    }

                }
                else
                {
                    var jsonInnerArray = (JArray) jsonItem;
                    NotifyRefreshers(jsonInnerArray); // recurse
                }
            }
        }

        private static void RefreshAll(Guid uniqueIdentifier)
        {
            var refresher = GetRefresher(uniqueIdentifier);
            refresher.RefreshAll();
        }

        private static void RefreshByGuid(Guid uniqueIdentifier, Guid id)
        {
            var refresher = GetRefresher(uniqueIdentifier);
            refresher.Refresh(id);
        }

        private static void RefreshById(Guid uniqueIdentifier, int id)
        {
            var refresher = GetRefresher(uniqueIdentifier);
            refresher.Refresh(id);
        }

        private static void RefreshByIds(Guid uniqueIdentifier, string jsonIds)
        {
            var refresher = GetRefresher(uniqueIdentifier);
            foreach (var id in JsonConvert.DeserializeObject<int[]>(jsonIds))
                refresher.Refresh(id);
        }

        private static void RefreshByJson(Guid uniqueIdentifier, string jsonPayload)
        {
            var refresher = GetJsonRefresher(uniqueIdentifier);
            refresher.Refresh(jsonPayload);
        }

        private static void RemoveById(Guid uniqueIdentifier, int id)
        {
            var refresher = GetRefresher(uniqueIdentifier);
            refresher.Remove(id);
        }

        #endregion
    }
}