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
        private readonly Dictionary<int, Guid> _id2Key = new Dictionary<int, Guid>();
        private readonly Dictionary<Guid, int> _key2Id = new Dictionary<Guid, int>();

        public IdkMap(IDatabaseUnitOfWorkProvider uowProvider)
        {
            _uowProvider = uowProvider;
        }

        // note - no need for uow, scope would be enough, but a pain to wire
        // note - for pure read-only we might want to *not* enforce a transaction?

        public Attempt<int> GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType)
        {
            int id;
            try
            {
                _locker.EnterReadLock();
                if (_key2Id.TryGetValue(key, out id)) return Attempt.Succeed(id);
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }

            int? val;
            using (var uow = _uowProvider.GetUnitOfWork())
            {
                val = uow.Database.ExecuteScalar<int?>("SELECT id FROM umbracoNode WHERE uniqueId=@id AND nodeObjectType=@nodeObjectType",
                    new { id = key, nodeObjectType = GetNodeObjectTypeGuid(umbracoObjectType) });
                uow.Commit();
            }

            if (val == null) return Attempt<int>.Fail();
            id = val.Value;

            try
            {
                _locker.EnterWriteLock();
                _id2Key[id] = key;
                _key2Id[key] = id;
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }

            return Attempt.Succeed(id);
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
            Guid key;
            try
            {
                _locker.EnterReadLock();
                if (_id2Key.TryGetValue(id, out key)) return Attempt.Succeed(key);
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }

            Guid? val;
            using (var uow = _uowProvider.GetUnitOfWork())
            {
                val = uow.Database.ExecuteScalar<Guid?>("SELECT uniqueId FROM umbracoNode WHERE id=@id AND nodeObjectType=@nodeObjectType",
                    new { id, nodeObjectType = GetNodeObjectTypeGuid(umbracoObjectType) });
                uow.Commit();
            }

            if (val == null) return Attempt<Guid>.Fail();
            key = val.Value;

            try
            {
                _locker.EnterWriteLock();
                _id2Key[id] = key;
                _key2Id[key] = id;
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }

            return Attempt.Succeed(key);
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
                Guid key;
                if (_id2Key.TryGetValue(id, out key) == false) return;
                _id2Key.Remove(id);
                _key2Id.Remove(key);
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
                int id;
                if (_key2Id.TryGetValue(key, out id) == false) return;
                _id2Key.Remove(id);
                _key2Id.Remove(key);
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }
    }
}