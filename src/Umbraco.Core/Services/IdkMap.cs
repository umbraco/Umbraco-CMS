using System;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class IdkMap
    {
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        private readonly Dictionary<int, TypedId<Guid>> _id2Key = new Dictionary<int, TypedId<Guid>>();
        private readonly Dictionary<Guid, TypedId<int>> _key2Id = new Dictionary<Guid, TypedId<int>>();

        public IdkMap(IDatabaseUnitOfWorkProvider uowProvider)
        {
            _uowProvider = uowProvider;
        }

        // note - no need for uow, scope would be enough, but a pain to wire
        // note - for pure read-only we might want to *not* enforce a transaction?

        public Attempt<int> GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType)
        {
            TypedId<int> id;
            try
            {
                _locker.EnterReadLock();
                if (_key2Id.TryGetValue(key, out id) && id.UmbracoObjectType == umbracoObjectType) return Attempt.Succeed(id.Id);
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }

            int? val;
            using (var uow = _uowProvider.GetUnitOfWork())
            {
                val = uow.Database.ExecuteScalar<int?>("SELECT id FROM umbracoNode WHERE uniqueId=@id AND (nodeObjectType=@type OR nodeObjectType=@reservation)",
                    new { id = key, type = GetNodeObjectTypeGuid(umbracoObjectType), reservation = Constants.ObjectTypes.IdReservationGuid });
                uow.Commit();
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
            TypedId<Guid> key;
            try
            {
                _locker.EnterReadLock();
                if (_id2Key.TryGetValue(id, out key) && key.UmbracoObjectType == umbracoObjectType) return Attempt.Succeed(key.Id);
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }

            Guid? val;
            using (var uow = _uowProvider.GetUnitOfWork())
            {
                val = uow.Database.ExecuteScalar<Guid?>("SELECT uniqueId FROM umbracoNode WHERE id=@id AND (nodeObjectType=@type OR nodeObjectType=@reservation)",
                    new { id, type = GetNodeObjectTypeGuid(umbracoObjectType), reservation = Constants.ObjectTypes.IdReservationGuid });
                uow.Commit();
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
                TypedId<Guid> key;
                if (_id2Key.TryGetValue(id, out key) == false) return;
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
                TypedId<int> id;
                if (_key2Id.TryGetValue(key, out id) == false) return;
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
            private readonly T _id;
            private readonly UmbracoObjectTypes _umbracoObjectType;

            public T Id
            {
                get { return _id; }
            }

            public UmbracoObjectTypes UmbracoObjectType
            {
                get { return _umbracoObjectType; }
            }

            public TypedId(T id, UmbracoObjectTypes umbracoObjectType)
            {
                _umbracoObjectType = umbracoObjectType;
                _id = id;
            }
        }
    }
}