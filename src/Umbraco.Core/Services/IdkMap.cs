using System;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services
{
    public class IdkMap
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

        public Attempt<int> GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType)
        {
            try
            {
                _locker.EnterReadLock();
                if (_key2Id.TryGetValue(key, out var id) && id.UmbracoObjectType == umbracoObjectType) return Attempt.Succeed(id.Id);
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }

            int? val;
            using (var scope = _scopeProvider.CreateScope())
            {
                var sql = scope.Database.SqlContext.Sql()
                    .Select<NodeDto>(x => x.NodeId).From<NodeDto>().Where<NodeDto>(x => x.UniqueId == key);
                
                if (umbracoObjectType != UmbracoObjectTypes.Unknown) // if unknow, don't include in query
                    sql = sql.Where<NodeDto>(x => x.NodeObjectType == GetNodeObjectTypeGuid(umbracoObjectType) || x.NodeObjectType == Constants.ObjectTypes.IdReservation); // fixme TEST the OR here!
                    
                val = scope.Database.ExecuteScalar<int?>(sql);
                scope.Complete();
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

        public Attempt<Guid> GetKeyForId(int id, UmbracoObjectTypes umbracoObjectType)
        {
            try
            {
                _locker.EnterReadLock();
                if (_id2Key.TryGetValue(id, out var key) && key.UmbracoObjectType == umbracoObjectType) return Attempt.Succeed(key.Id);
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }

            Guid? val;
            using (var scope = _scopeProvider.CreateScope())
            {
                var sql = scope.Database.SqlContext.Sql()
                    .Select<NodeDto>(x => x.UniqueId).From<NodeDto>().Where<NodeDto>(x => x.NodeId == id);
                
                if (umbracoObjectType != UmbracoObjectTypes.Unknown) // if unknow, don't include in query
                    sql = sql.Where<NodeDto>(x => x.NodeObjectType == GetNodeObjectTypeGuid(umbracoObjectType) || x.NodeObjectType == Constants.ObjectTypes.IdReservation); // fixme TEST the OR here!
                    
                val = scope.Database.ExecuteScalar<Guid?>(sql);
                scope.Complete();
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

        private struct TypedId<T>
        {
            public T Id { get; }

            public UmbracoObjectTypes UmbracoObjectType { get; }

            public TypedId(T id, UmbracoObjectTypes umbracoObjectType)
            {
                UmbracoObjectType = umbracoObjectType;
                Id = id;
            }
        }
    }
}
