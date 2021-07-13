using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core.Models;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services
{
    public class IdkMap : IDisposable
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        private readonly Dictionary<int, TypedId<Guid>> _id2Key = new Dictionary<int, TypedId<Guid>>();
        private readonly Dictionary<Guid, TypedId<int>> _key2Id = new Dictionary<Guid, TypedId<int>>();

        public IdkMap(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        // note - for pure read-only we might want to *not* enforce a transaction?

        // notes
        //
        // - this class assumes that the id/guid map is unique; that is, if an id and a guid map
        //   to each other, then the id will never map to another guid, and the guid will never map
        //   to another id
        //
        // - cache is cleared by MediaCacheRefresher, UnpublishedPageCacheRefresher, and other
        //   refreshers - because id/guid map is unique, we only clear to avoid leaking memory, 'cos
        //   we don't risk caching obsolete values - and only when actually deleting
        //
        // - we do NOT prefetch anything from database
        //
        // - NuCache maintains its own id/guid map for content & media items
        //   it does *not* populate the idk map, because it directly uses its own map
        //   still, it provides mappers so that the idk map can benefit from them
        //   which means there will be some double-caching at some point ??
        //
        // - when a request comes in:
        //   if the idkMap already knows about the map, it returns the value
        //   else it tries the published cache via mappers
        //   else it hits the database

        private readonly ConcurrentDictionary<UmbracoObjectTypes, (Func<int, Guid> id2key, Func<Guid, int> key2id)> _dictionary
            = new ConcurrentDictionary<UmbracoObjectTypes, (Func<int, Guid> id2key, Func<Guid, int> key2id)>();
        private bool _disposedValue;

        internal void SetMapper(UmbracoObjectTypes umbracoObjectType, Func<int, Guid> id2key, Func<Guid, int> key2id)
        {
            _dictionary[umbracoObjectType] = (id2key, key2id);
        }

        internal void Populate(IEnumerable<(int id, Guid key)> pairs, UmbracoObjectTypes umbracoObjectType)
        {
            try
            {
                _locker.EnterWriteLock();
                foreach (var pair in pairs)
                {

                    _id2Key[pair.id] = new TypedId<Guid>(pair.key, umbracoObjectType);
                    _key2Id[pair.key] = new TypedId<int>(pair.id, umbracoObjectType);
                }
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }

#if POPULATE_FROM_DATABASE
        private void PopulateLocked()
        {
            // don't if not empty
            if (_key2Id.Count > 0) return;

            using (var scope = _scopeProvider.CreateScope())
            {
                // populate content and media items
                var types = new[] { Constants.ObjectTypes.Document, Constants.ObjectTypes.Media };
                var values = scope.Database.Query<TypedIdDto>("SELECT id, uniqueId, nodeObjectType FROM umbracoNode WHERE nodeObjectType IN @types", new { types });
                foreach (var value in values)
                {
                    var umbracoObjectType = ObjectTypes.GetUmbracoObjectType(value.NodeObjectType);
                    _id2Key.Add(value.Id, new TypedId<Guid>(value.UniqueId, umbracoObjectType));
                    _key2Id.Add(value.UniqueId, new TypedId<int>(value.Id, umbracoObjectType));
                }
            }
        }

        private Attempt<int> PopulateAndGetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType)
        {
            try
            {
                _locker.EnterWriteLock();

                PopulateLocked();

                return _key2Id.TryGetValue(key, out var id) && id.UmbracoObjectType == umbracoObjectType
                    ? Attempt.Succeed(id.Id)
                    : Attempt<int>.Fail();

            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }

        private Attempt<Guid> PopulateAndGetKeyForId(int id, UmbracoObjectTypes umbracoObjectType)
        {
            try
            {
                _locker.EnterWriteLock();

                PopulateLocked();

                return _id2Key.TryGetValue(id, out var key) && key.UmbracoObjectType == umbracoObjectType
                    ? Attempt.Succeed(key.Id)
                    : Attempt<Guid>.Fail();
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }
#endif

        public Attempt<int> GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType)
        {
            bool empty;

            try
            {
                _locker.EnterReadLock();
                if (_key2Id.TryGetValue(key, out var id) && id.UmbracoObjectType == umbracoObjectType) return Attempt.Succeed(id.Id);
                empty = _key2Id.Count == 0;
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }

#if POPULATE_FROM_DATABASE
            // if cache is empty and looking for a document or a media,
            // populate the cache at once and return what we found
            if (empty && (umbracoObjectType == UmbracoObjectTypes.Document || umbracoObjectType == UmbracoObjectTypes.Media))
                return PopulateAndGetIdForKey(key, umbracoObjectType);
#endif

            // optimize for read speed: reading database outside a lock means that we could read
            // multiple times, but we don't lock the cache while accessing the database = better

            int? val = null;

            if (_dictionary.TryGetValue(umbracoObjectType, out var mappers))
                if ((val = mappers.key2id(key)) == default(int)) val = null;

            if (val == null)
            {
                using (var scope = _scopeProvider.CreateScope())
                {
                    //if it's unknown don't include the nodeObjectType in the query
                    if (umbracoObjectType == UmbracoObjectTypes.Unknown)
                    {
                        val = scope.Database.ExecuteScalar<int?>("SELECT id FROM umbracoNode WHERE uniqueId=@id", new { id = key});
                    }
                    else
                    {
                        val = scope.Database.ExecuteScalar<int?>("SELECT id FROM umbracoNode WHERE uniqueId=@id AND (nodeObjectType=@type OR nodeObjectType=@reservation)",
                            new { id = key, type = GetNodeObjectTypeGuid(umbracoObjectType), reservation = Constants.ObjectTypes.IdReservation });
                    }
                    scope.Complete();
                }
            }

            if (val == null) return Attempt<int>.Fail();

            // cache reservations, when something is saved this cache is cleared anyways
            //if (umbracoObjectType == UmbracoObjectTypes.IdReservation)
            //    Attempt.Succeed(val.Value);

            try
            {
                _locker.EnterWriteLock();
                _id2Key[val.Value] = new TypedId<Guid>(key, umbracoObjectType);
                _key2Id[key] = new TypedId<int>(val.Value, umbracoObjectType);
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }

            return Attempt.Succeed(val.Value);
        }

        public Attempt<int> GetIdForUdi(Udi udi)
        {
            var guidUdi = udi as GuidUdi;
            if (guidUdi == null)
                return Attempt<int>.Fail();

            var umbracoType = Constants.UdiEntityType.ToUmbracoObjectType(guidUdi.EntityType);
            return GetIdForKey(guidUdi.Guid, umbracoType);
        }

        public Attempt<Udi> GetUdiForId(int id, UmbracoObjectTypes umbracoObjectType)
        {
            var keyAttempt = GetKeyForId(id, umbracoObjectType);
            return keyAttempt
                ? Attempt.Succeed<Udi>(new GuidUdi(Constants.UdiEntityType.FromUmbracoObjectType(umbracoObjectType), keyAttempt.Result))
                : Attempt<Udi>.Fail();
        }

        public Attempt<Guid> GetKeyForId(int id, UmbracoObjectTypes umbracoObjectType)
        {
            bool empty;

            try
            {
                _locker.EnterReadLock();
                if (_id2Key.TryGetValue(id, out var key) && key.UmbracoObjectType == umbracoObjectType) return Attempt.Succeed(key.Id);
                empty = _id2Key.Count == 0;
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }

#if POPULATE_FROM_DATABASE
            // if cache is empty and looking for a document or a media,
            // populate the cache at once and return what we found
            if (empty && (umbracoObjectType == UmbracoObjectTypes.Document || umbracoObjectType == UmbracoObjectTypes.Media))
                return PopulateAndGetKeyForId(id, umbracoObjectType);
#endif

            // optimize for read speed: reading database outside a lock means that we could read
            // multiple times, but we don't lock the cache while accessing the database = better

            Guid? val = null;

            if (_dictionary.TryGetValue(umbracoObjectType, out var mappers))
                if ((val = mappers.id2key(id)) == default(Guid)) val = null;

            if (val == null)
            {
                using (var scope = _scopeProvider.CreateScope())
                {
                    //if it's unknown don't include the nodeObjectType in the query
                    if (umbracoObjectType == UmbracoObjectTypes.Unknown)
                    {
                        val = scope.Database.ExecuteScalar<Guid?>("SELECT uniqueId FROM umbracoNode WHERE id=@id", new { id });
                    }
                    else
                    {
                        val = scope.Database.ExecuteScalar<Guid?>("SELECT uniqueId FROM umbracoNode WHERE id=@id AND (nodeObjectType=@type OR nodeObjectType=@reservation)",
                            new { id, type = GetNodeObjectTypeGuid(umbracoObjectType), reservation = Constants.ObjectTypes.IdReservation });
                    }
                    scope.Complete();
                }
            }

            if (val == null) return Attempt<Guid>.Fail();

            // cache reservations, when something is saved this cache is cleared anyways
            //if (umbracoObjectType == UmbracoObjectTypes.IdReservation)
            //    Attempt.Succeed(val.Value);

            try
            {
                _locker.EnterWriteLock();
                _id2Key[id] = new TypedId<Guid>(val.Value, umbracoObjectType);
                _key2Id[val.Value] = new TypedId<int>(id, umbracoObjectType);
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }

            return Attempt.Succeed(val.Value);
        }

        private static Guid GetNodeObjectTypeGuid(UmbracoObjectTypes umbracoObjectType)
        {
            var guid = umbracoObjectType.GetGuid();
            if (guid == Guid.Empty)
                throw new NotSupportedException("Unsupported object type (" + umbracoObjectType + ").");
            return guid;
        }

        // invoked on UnpublishedPageCacheRefresher.RefreshAll
        // anything else will use the id-specific overloads
        public void ClearCache()
        {
            try
            {
                _locker.EnterWriteLock();
                _id2Key.Clear();
                _key2Id.Clear();
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }

        public void ClearCache(int id)
        {
            try
            {
                _locker.EnterWriteLock();
                if (_id2Key.TryGetValue(id, out var key) == false) return;
                _id2Key.Remove(id);
                _key2Id.Remove(key.Id);
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }

        public void ClearCache(Guid key)
        {
            try
            {
                _locker.EnterWriteLock();
                if (_key2Id.TryGetValue(key, out var id) == false) return;
                _id2Key.Remove(id.Id);
                _key2Id.Remove(key);
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }

        // ReSharper disable ClassNeverInstantiated.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class TypedIdDto
        {
            public int Id { get; set; }
            public Guid UniqueId { get; set; }
            public Guid NodeObjectType { get; set; }
        }
        // ReSharper restore ClassNeverInstantiated.Local
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        private struct TypedId<T>
        {
            public TypedId(T id, UmbracoObjectTypes umbracoObjectType)
            {
                UmbracoObjectType = umbracoObjectType;
                Id = id;
            }

            public UmbracoObjectTypes UmbracoObjectType { get; }

            public T Id { get; }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _locker.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
