using System;
using System.Collections;

namespace Umbraco.Core.Cache
{
    internal class DictionaryCacheWrapper : IEnumerable
    {
        private readonly IEnumerable _inner;
        private readonly Func<object, object> _get;
        private readonly Action<object> _remove;

        public DictionaryCacheWrapper(
            IEnumerable inner,
            Func<object, object> get,
            Action<object> remove)
        {
            _inner = inner;
            _get = get;
            _remove = remove;
        }

        public object this[object key]
        {
            get
            {
                return Get(key);
            }            
        }

        public object Get(object key)
        {
            return _get(key);
        }

        public void Remove(object key)
        {
            _remove(key);
        }

        public IEnumerator GetEnumerator()
        {
            return _inner.GetEnumerator();
        }
    }
}