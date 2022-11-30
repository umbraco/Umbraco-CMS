// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentTypeSortBuilder
    : ChildBuilderBase<ContentTypeBuilder, ContentTypeSort>,
        IWithIdBuilder,
        IWithAliasBuilder,
        IWithSortOrderBuilder
{
    private string _alias;
    private int? _id;
    private int? _sortOrder;

    public ContentTypeSortBuilder()
        : base(null)
    {
    }

    public ContentTypeSortBuilder(ContentTypeBuilder parentBuilder)
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

    int? IWithSortOrderBuilder.SortOrder
    {
        get => _sortOrder;
        set => _sortOrder = value;
    }

    public override ContentTypeSort Build()
    {
        var id = _id ?? 1;
        var alias = _alias ?? Guid.NewGuid().ToString().ToCamelCase();
        var sortOrder = _sortOrder ?? 0;

        return new ContentTypeSort(new Lazy<int>(() => id), sortOrder, alias);
    }
}
