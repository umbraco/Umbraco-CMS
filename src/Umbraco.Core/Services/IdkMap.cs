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
            var compositeKey = new Key2IdCompositeKey() { Key = key, UmbracoObjectType = umbracoObjectType };
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
                var reversedCompositeKey = new Id2KeyCompositeKey { Id = id, UmbracoObjectType = umbracoObjectType };
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
            var compositeKey = new Id2KeyCompositeKey() {Id = id, UmbracoObjectType = umbracoObjectType};
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
                var reversedCompositeKey = new Key2IdCompositeKey {Key = key, UmbracoObjectType = umbracoObjectType};
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
                var match = _id2Key.Keys.SingleOrDefault(x => x.Id == id);
                if (match == null) return;
                var key = _id2Key[match];
                var reversedCompositeKey = new Key2IdCompositeKey { Key = key, UmbracoObjectType = match.UmbracoObjectType };
                _id2Key.Remove(match);
                _key2Id.Remove(reversedCompositeKey);
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
                var match = _key2Id.Keys.SingleOrDefault(x => x.Key == key);
                if (match == null) return;
                var id = _key2Id[match];
                var reversedCompositeKey = new Id2KeyCompositeKey {Id = id, UmbracoObjectType = match.UmbracoObjectType};
                _id2Key.Remove(reversedCompositeKey);
                _key2Id.Remove(match);
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }

        internal class Id2KeyCompositeKey
        {
            public int Id { get; set; }
            public UmbracoObjectTypes UmbracoObjectType { get; set; }

            protected bool Equals(Id2KeyCompositeKey other)
            {
                return Id == other.Id && UmbracoObjectType == other.UmbracoObjectType;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Id2KeyCompositeKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Id * 397) ^ (int) UmbracoObjectType;
                }
            }
        }

        internal class Key2IdCompositeKey
        {
            public Guid Key { get; set; }
            public UmbracoObjectTypes UmbracoObjectType { get; set; }

            protected bool Equals(Key2IdCompositeKey other)
            {
                return Key.Equals(other.Key) && UmbracoObjectType == other.UmbracoObjectType;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Key2IdCompositeKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Key.GetHashCode() * 397) ^ (int) UmbracoObjectType;
                }
            }
        }
    }
}