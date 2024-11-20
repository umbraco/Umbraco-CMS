// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentTypeSortBuilder<TBuilder>
    : ChildBuilderBase<TBuilder, ContentTypeSort>,
        IWithKeyBuilder,
        IWithAliasBuilder,
        IWithSortOrderBuilder
{
    private Guid? _key;
    private string _alias;
    private int? _sortOrder;

    public ContentTypeSortBuilder(TBuilder parentBuilder)
        : base(parentBuilder)
    {
    }

    string IWithAliasBuilder.Alias
    {
        get => _alias;
        set => _alias = value;
    }

    Guid? IWithKeyBuilder.Key
    {
        get => _key;
        set => _key = value;
    }

    int? IWithSortOrderBuilder.SortOrder
    {
        get => _sortOrder;
        set => _sortOrder = value;
    }

    public override ContentTypeSort Build()
    {
        var alias = _alias ?? Guid.NewGuid().ToString().ToCamelCase();
        var sortOrder = _sortOrder ?? 0;
        var key = _key ?? Guid.NewGuid();

        return new ContentTypeSort(key, sortOrder, alias);
    }
}
