// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Cms.Tests.Common.Builders;

public class GenericCollectionBuilder<TBuilder, T>
    : ChildBuilderBase<TBuilder, IEnumerable<T>>
{
    private readonly IList<T> _collection;

    public GenericCollectionBuilder(TBuilder parentBuilder)
        : base(parentBuilder) => _collection = new List<T>();

    public override IEnumerable<T> Build()
    {
        var collection = _collection?.ToList() ?? Enumerable.Empty<T>();
        return collection;
    }

    public GenericCollectionBuilder<TBuilder, T> WithValue(T value)
    {
        _collection.Add(value);
        return this;
    }
}
