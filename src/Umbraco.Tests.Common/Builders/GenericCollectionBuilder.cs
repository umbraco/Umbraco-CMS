using System.Collections.Generic;

namespace Umbraco.Tests.Common.Builders
{
    public class GenericCollectionBuilder<TBuilder, T>
        : ChildBuilderBase<TBuilder, IEnumerable<T>>
    {
        private readonly IList<T> _collection;

        public GenericCollectionBuilder(TBuilder parentBuilder) : base(parentBuilder)
        {
            _collection = new List<T>();
        }        

        public override IEnumerable<T> Build()
        {
            return _collection;
        }

        public GenericCollectionBuilder<TBuilder, T> WithValue(T value)
        {
            _collection.Add(value);
            return this;
        }
    }
}
