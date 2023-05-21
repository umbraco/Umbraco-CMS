// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;

namespace Umbraco.Cms.Tests.Common.Builders;

public class GenericDictionaryBuilder<TBuilder, TKey, TValue>
    : ChildBuilderBase<TBuilder, IDictionary<TKey, TValue>>
{
    private readonly IDictionary<TKey, TValue> _dictionary;

    public GenericDictionaryBuilder(TBuilder parentBuilder)
        : base(parentBuilder) => _dictionary = new Dictionary<TKey, TValue>();

    public override IDictionary<TKey, TValue> Build() => _dictionary == null
        ? new Dictionary<TKey, TValue>()
        : new Dictionary<TKey, TValue>(_dictionary);

    public GenericDictionaryBuilder<TBuilder, TKey, TValue> WithKeyValue(TKey key, TValue value)
    {
        _dictionary.Add(key, value);
        return this;
    }
}
