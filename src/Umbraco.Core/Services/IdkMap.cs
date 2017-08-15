using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class IdkMap
    {
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private readonly Dictionary<Id2KeyCompositeKey, Guid> _id2Key = new Dictionary<Id2KeyCompositeKey, Guid>();
        private readonly Dictionary<Key2IdCompositeKey, int> _key2Id = new Dictionary<Key2IdCompositeKey, int>();

        public IdkMap(IDatabaseUnitOfWorkProvider uowProvider)
        {
            _uowProvider = uowProvider;
        }

        // note - no need for uow, scope would be enough, but a pain to wire
        // note - for pure read-only we might want to *not* enforce a transaction?

        public Attempt<int> GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType)
        {
            var compositeKey = new Key2IdCompositeKey(key, umbracoObjectType);
            int id;
            try
            {
                _locker.EnterReadLock();
                if (_key2Id.TryGetValue(compositeKey, out id)) return Attempt.Succeed(id);
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
                var reversedCompositeKey = new Id2KeyCompositeKey(id, umbracoObjectType);
                _locker.EnterWriteLock();
                _id2Key[reversedCompositeKey] = key;
                _key2Id[compositeKey] = id;
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
            var compositeKey = new Id2KeyCompositeKey(id, umbracoObjectType);
            Guid key;
            try
            {
                _locker.EnterReadLock();
                if (_id2Key.TryGetValue(compositeKey, out key)) return Attempt.Succeed(key);
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
                var reversedCompositeKey = new Key2IdCompositeKey(key, umbracoObjectType);
                _locker.EnterWriteLock();
                _id2Key[compositeKey] = key;
                _key2Id[reversedCompositeKey] = id;
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
                foreach (var compositeKey in _id2Key.Keys)
                {
                    if (compositeKey.Id == id)
                    {
                        var guid = _id2Key[compositeKey];
                        var reversedCompositeKey = new Key2IdCompositeKey(guid, compositeKey.UmbracoObjectType);
                        _id2Key.Remove(compositeKey);
                        _key2Id.Remove(reversedCompositeKey);
                        break;
                    }
                }
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
                foreach (var compositeKey in _key2Id.Keys)
                {
                    if (compositeKey.Key == key)
                    {
                        var id = _key2Id[compositeKey];
                        var reversedCompositeKey = new Id2KeyCompositeKey(id, compositeKey.UmbracoObjectType);
                        _id2Key.Remove(reversedCompositeKey);
                        _key2Id.Remove(compositeKey);
                        break;
                    }
                }
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }

        private struct Id2KeyCompositeKey
        {
            private readonly int _id;
            private readonly UmbracoObjectTypes _umbracoObjectType;

            public int Id
            {
                get { return _id; }
            }

            public UmbracoObjectTypes UmbracoObjectType
            {
                get { return _umbracoObjectType; }
            }

            public Id2KeyCompositeKey(int id, UmbracoObjectTypes umbracoObjectType)
            {
                _id = id;
                _umbracoObjectType = umbracoObjectType;
            }

            private bool Equals(Id2KeyCompositeKey other)
            {
                return _id == other._id && _umbracoObjectType == other._umbracoObjectType;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is Id2KeyCompositeKey && Equals((Id2KeyCompositeKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_id * 397) ^ (int) _umbracoObjectType;
                }
            }
        }

        private struct Key2IdCompositeKey
        {
            private readonly Guid _key;
            private readonly UmbracoObjectTypes _umbracoObjectType;

            public Guid Key
            {
                get { return _key; }
            }

            public UmbracoObjectTypes UmbracoObjectType
            {
                get { return _umbracoObjectType; }
            }

            public Key2IdCompositeKey(Guid key, UmbracoObjectTypes umbracoObjectType)
            {
                _key = key;
                _umbracoObjectType = umbracoObjectType;
            }

            private bool Equals(Key2IdCompositeKey other)
            {
                return _key.Equals(other._key) && _umbracoObjectType == other._umbracoObjectType;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is Key2IdCompositeKey && Equals((Key2IdCompositeKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_key.GetHashCode() * 397) ^ (int) _umbracoObjectType;
                }
            }
        }
    }
}