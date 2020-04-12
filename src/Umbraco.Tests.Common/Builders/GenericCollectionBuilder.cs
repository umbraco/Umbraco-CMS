using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Tests.Common.Builders
{
    public class GenericCollectionBuilder<TBuilder, T>
        : ChildBuilderBase<TBuilder, IEnumerable<T>>
    {
        private IList<T> _collection;

        public GenericCollectionBuilder(TBuilder parentBuilder) : base(parentBuilder)
        {
        }        

        public override IEnumerable<T> Build()
        {
            var collection = _collection?.ToList() ?? Enumerable.Empty<T>();
            Reset();
            return collection;
        }

        protected override void Reset()
        {
            _collection = null;
        }

        public GenericCollectionBuilder<TBuilder, T> WithValue(T value)
        {
            if (_collection == null)
            {
                _collection = new List<T>();
            }

            _collection.Add(value);
            return this;
        }
    }
}
