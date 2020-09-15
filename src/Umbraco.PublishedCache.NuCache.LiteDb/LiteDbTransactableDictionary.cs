using LiteDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.PublishedCache.NuCache;

namespace Umbraco.PublishedCache.NuCache.LiteDb
{
    public class LiteDbTransactableDictionary<Tkey, TValue> : ITransactableDictionary<Tkey, TValue> where TValue : IKey<Tkey>
    {
        private readonly string _connectionString;
        private readonly string _collectionName;
        private readonly bool _isReadOnly;
        private LiteDatabase _db;

        public LiteDbTransactableDictionary(string connectionString, string collectionName, bool isReadOnly = false)
        {
            _connectionString = connectionString;
            _collectionName = collectionName;
            _isReadOnly = isReadOnly;
            _db = new LiteDatabase(_connectionString, BsonMapper.Global);
            _db.GetCollection<TValue>(_collectionName);
        }

        public void BeginTransaction()
        {
            _db.BeginTrans();
        }

        public TValue this[Tkey key]
        {
            get
            {
                var col = _db.GetCollection<TValue>(_collectionName);
                return col.FindById(new BsonValue(key));
            }
            set {
                var col = _db.GetCollection<TValue>(_collectionName);
                col.Insert(value);
            }
        }

        public ICollection<Tkey> Keys
        {
            get
            {
                // Get a collection (or create, if doesn't exist)
                var col = _db.GetCollection<IKey<Tkey>>(_collectionName);

                // Use LINQ to query documents (filter, sort, transform)
                var results = col.Query()
                    .Select(x => x.Key)
                    .ToList();

                return results;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                // Get a collection (or create, if doesn't exist)
                var col = _db.GetCollection<TValue>(_collectionName);

                // Use LINQ to query documents (filter, sort, transform)
                var results = col.Query()
                    .ToList();

                return results;
            }
        }

        public int Count
        {
            get
            {
                var col = _db.GetCollection<TValue>(_collectionName);

                // Use LINQ to query documents (filter, sort, transform)
                var result = col.Query().Count();

                return result;
            }
        }

        public bool IsReadOnly => _isReadOnly;

        public void Add(Tkey key, TValue value)
        {
            var col = _db.GetCollection<TValue>(_collectionName);
            col.Insert(value);
        }

        public void Add(KeyValuePair<Tkey, TValue> item)
        {
            var col = _db.GetCollection<TValue>(_collectionName);
            col.Insert(item.Value);
        }

        public void Clear()
        {
            var col = _db.GetCollection<TValue>(_collectionName);
            col.DeleteAll();
        }

        public void Commit()
        {
            _db.Commit();
        }

        public bool Contains(KeyValuePair<Tkey, TValue> item)
        {
            var col = _db.GetCollection<TValue>(_collectionName);
            return col.Query().Where(x => x.Key.Equals(item.Key))
                .Exists();
        }

        public bool ContainsKey(Tkey key)
        {
            var col = _db.GetCollection<TValue>(_collectionName);
            return col.Query().Where(x => x.Key.Equals(key))
                .Exists();
        }

        public void CopyTo(KeyValuePair<Tkey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public void DeleteLocalFiles()
        {
            _db.DropCollection(_collectionName);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public IEnumerator<KeyValuePair<Tkey, TValue>> GetEnumerator()
        {
            var col = _db.GetCollection<TValue>(_collectionName);
            var e = col.FindAll();
            var b = e.ToList();
            return e.Select(x=> new KeyValuePair<Tkey,TValue>(x.Key,x)).GetEnumerator();
        }

        public bool LocalFilesExist()
        {
            return _db.CollectionExists(_collectionName);
        }

        public bool Remove(Tkey key)
        {
            var col = _db.GetCollection<TValue>(_collectionName);
            return col.Delete(new BsonValue(key));
        }

        public bool Remove(KeyValuePair<Tkey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            _db.Rollback();
        }

        public bool TryGetValue(Tkey key, out TValue value)
        {
            var col = _db.GetCollection<TValue>(_collectionName);
            value = col.FindById(new BsonValue(key));
            return value != null;
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            var col = _db.GetCollection<TValue>(_collectionName);
            return col.FindAll().GetEnumerator();
        }

        public bool TryRemove(Tkey key, out TValue unused)
        {
            var col = _db.GetCollection<TValue>(_collectionName);
            unused = col.FindById(new BsonValue(key));
            if (unused != null)
            {
                col.Delete(new BsonValue(key));
                return true;
            }
            return false;
        }
    }
}
