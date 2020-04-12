using System.Collections.Generic;

namespace Umbraco.Tests.Common.Builders
{
    public class GenericDictionaryBuilder<TBuilder, TKey, TValue>
        : ChildBuilderBase<TBuilder, IDictionary<TKey, TValue>>
    {
        private IDictionary<TKey, TValue> _dictionary;

        public GenericDictionaryBuilder(TBuilder parentBuilder) : base(parentBuilder)
        {
        }        

        public override IDictionary<TKey, TValue> Build()
        {
            var dictionary = _dictionary == null
                ? new Dictionary<TKey, TValue>()
                : new Dictionary<TKey, TValue>(_dictionary);
            Reset();
            return dictionary;
        }

        protected override void Reset()
        {
            _dictionary = null;
        }


        public GenericDictionaryBuilder<TBuilder, TKey, TValue> WithKeyValue(TKey key, TValue value)
        {
            if (_dictionary == null)
            {
                _dictionary = new Dictionary<TKey, TValue>();
            }

            _dictionary.Add(key, value);
            return this;
        }
    }
}
