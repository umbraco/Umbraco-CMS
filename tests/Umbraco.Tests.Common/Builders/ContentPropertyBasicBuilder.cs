// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentPropertyBasicBuilder<TParent> : ChildBuilderBase<TParent, ContentPropertyBasic>,
    IWithIdBuilder, IWithAliasBuilder
{
    private string _alias;
    private int? _id;
    private object _value;

    public ContentPropertyBasicBuilder(TParent parentBuilder)
        : base(parentBuilder)
    {
    }

    string IWithAliasBuilder.Alias
    {
        get => _alias;
        set => _alias = value;
    }

    int? IWithIdBuilder.Id
    {
        get => _id;
        set => _id = value;
    }

    public override ContentPropertyBasic Build()
    {
        var alias = _alias;
        var id = _id ?? 0;
        var value = _value;

        return new ContentPropertyBasic { Alias = alias, Id = id, Value = value };
    }

    public ContentPropertyBasicBuilder<TParent> WithValue(object value)
    {
        _value = value;
        return this;
    }
}
