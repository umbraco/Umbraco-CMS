// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class MacroPropertyBuilder
    : ChildBuilderBase<MacroBuilder, IMacroProperty>,
        IWithIdBuilder,
        IWithKeyBuilder,
        IWithAliasBuilder,
        IWithNameBuilder,
        IWithSortOrderBuilder
{
    private string _alias;
    private string _editorAlias;
    private int? _id;
    private Guid? _key;
    private string _name;
    private int? _sortOrder;

    public MacroPropertyBuilder(MacroBuilder parentBuilder)
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

    Guid? IWithKeyBuilder.Key
    {
        get => _key;
        set => _key = value;
    }

    string IWithNameBuilder.Name
    {
        get => _name;
        set => _name = value;
    }

    int? IWithSortOrderBuilder.SortOrder
    {
        get => _sortOrder;
        set => _sortOrder = value;
    }

    public MacroPropertyBuilder WithEditorAlias(string editorAlias)
    {
        _editorAlias = editorAlias;
        return this;
    }

    public override IMacroProperty Build()
    {
        var id = _id ?? 1;
        var name = _name ?? Guid.NewGuid().ToString();
        var alias = _alias ?? name.ToCamelCase();
        var key = _key ?? Guid.NewGuid();
        var sortOrder = _sortOrder ?? 0;
        var editorAlias = _editorAlias ?? string.Empty;

        return new MacroProperty(id, key, alias, name, sortOrder, editorAlias);
    }
}
