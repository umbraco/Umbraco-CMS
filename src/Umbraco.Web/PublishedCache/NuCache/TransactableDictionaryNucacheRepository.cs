using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public class TransactableDictionaryNucacheRepository : INucacheRepositoryBase<int,ContentNodeKit>,INucacheMediaRepository,INucacheDocumentRepository
    {
        private readonly ITransactableDictionary<int, ContentNodeKit> _transactableDictionary;

        public TransactableDictionaryNucacheRepository(ITransactableDictionary<int, ContentNodeKit> transactableDictionary)
        {
            _transactableDictionary = transactableDictionary;
        }
        public void Init()
        {

        }
        public ICollection<ContentNodeKit> GetAllSorted()
        {
            var kits = _transactableDictionary.Select(x => x.Value)
                   .OrderBy(x => x.Node.Level)
                   .ThenBy(x => x.Node.ParentContentId)
                   .ThenBy(x => x.Node.SortOrder) // IMPORTANT sort by level + parentId + sortOrder
                   .ToList();
            return kits;
        }
        public bool EnsureEnvironment(out IEnumerable<string> errors)
        {
            return _transactableDictionary.EnsureEnvironment(out errors);
        }


        #region  ITransactableDictionary<int, ContentNodeKit>
        public ContentNodeKit this[int key] { get => _transactableDictionary[key]; set => _transactableDictionary[key] = value; }

        public ICollection<int> Keys => _transactableDictionary.Keys;

        public ICollection<ContentNodeKit> Values => _transactableDictionary.Values;

        public int Count => _transactableDictionary.Count;

        public bool IsReadOnly => _transactableDictionary.IsReadOnly;

        public void Add(int key, ContentNodeKit value)
        {
            _transactableDictionary.Add(key, value);
        }

        public void Add(KeyValuePair<int, ContentNodeKit> item)
        {
            _transactableDictionary.Add(item);
        }

        public ITransactionScope BeginTransaction()
        {
            return _transactableDictionary.BeginTransaction();
        }

        public void Clear()
        {
            _transactableDictionary.Clear();
        }

        public bool Contains(KeyValuePair<int, ContentNodeKit> item)
        {
            return _transactableDictionary.Contains(item);
        }

        public bool ContainsKey(int key)
        {
            return _transactableDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<int, ContentNodeKit>[] array, int arrayIndex)
        {
            _transactableDictionary.CopyTo(array, arrayIndex);
        }

        public void Dispose()
        {
            _transactableDictionary.Dispose();
        }

        public void Drop()
        {
            _transactableDictionary.Drop();
        }


        public IEnumerator<KeyValuePair<int, ContentNodeKit>> GetEnumerator()
        {
            return _transactableDictionary.GetEnumerator();
        }

        public bool IsPopulated()
        {
            return _transactableDictionary.IsPopulated();
        }

        public bool Remove(int key)
        {
            return _transactableDictionary.Remove(key);
        }

        public bool Remove(KeyValuePair<int, ContentNodeKit> item)
        {
            return _transactableDictionary.Remove(item);
        }

        public bool TryGetValue(int key, out ContentNodeKit value)
        {
            return _transactableDictionary.TryGetValue(key, out value);
        }

        public bool TryRemove(int key, out ContentNodeKit value)
        {
            return _transactableDictionary.TryRemove(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_transactableDictionary).GetEnumerator();
        }

       

        #endregion
    }
}
