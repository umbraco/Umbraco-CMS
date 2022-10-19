// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PropertyGroupBuilder : PropertyGroupBuilder<NullPropertyGroupBuilderParent>
{
    public PropertyGroupBuilder()
        : base(null)
    {
    }
}

public class NullPropertyGroupBuilderParent : IBuildPropertyGroups
{
}

public class PropertyGroupBuilder<TParent>
    : ChildBuilderBase<TParent, PropertyGroup>,
        IBuildPropertyTypes,
        IWithIdBuilder,
        IWithKeyBuilder,
        IWithCreateDateBuilder,
        IWithUpdateDateBuilder,
        IWithAliasBuilder,
        IWithNameBuilder,
        IWithSortOrderBuilder,
        IWithSupportsPublishing
    where TParent : IBuildPropertyGroups
{
    private readonly List<PropertyTypeBuilder<PropertyGroupBuilder<TParent>>> _propertyTypeBuilders = new();
    private string _alias;
    private DateTime? _createDate;

    private int? _id;
    private Guid? _key;
    private string _name;
    private PropertyTypeCollection _propertyTypeCollection;
    private int? _sortOrder;
    private bool? _supportsPublishing;
    private DateTime? _updateDate;

    public PropertyGroupBuilder(TParent parentBuilder)
        : base(parentBuilder)
    {
    }

    string IWithAliasBuilder.Alias
    {
        get => _alias;
        set => _alias = value;
    }

    DateTime? IWithCreateDateBuilder.CreateDate
    {
        get => _createDate;
        set => _createDate = value;
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

    bool? IWithSupportsPublishing.SupportsPublishing
    {
        get => _supportsPublishing;
        set => _supportsPublishing = value;
    }

    DateTime? IWithUpdateDateBuilder.UpdateDate
    {
        get => _updateDate;
        set => _updateDate = value;
    }

    public PropertyGroupBuilder<TParent> WithPropertyTypeCollection(PropertyTypeCollection propertyTypeCollection)
    {
        _propertyTypeCollection = propertyTypeCollection;
        return this;
    }

    public PropertyTypeBuilder<PropertyGroupBuilder<TParent>> AddPropertyType()
    {
        var builder = new PropertyTypeBuilder<PropertyGroupBuilder<TParent>>(this);
        _propertyTypeBuilders.Add(builder);
        return builder;
    }

    public override PropertyGroup Build()
    {
        var id = _id ?? 0;
        var key = _key ?? Guid.NewGuid();
        var createDate = _createDate ?? DateTime.Now;
        var updateDate = _updateDate ?? DateTime.Now;
        var alias = _alias ?? Guid.NewGuid().ToString();
        var name = _name ?? Guid.NewGuid().ToString();
        var sortOrder = _sortOrder ?? 0;
        var supportsPublishing = _supportsPublishing ?? false;

        PropertyTypeCollection propertyTypeCollection;
        if (_propertyTypeCollection != null)
        {
            propertyTypeCollection = _propertyTypeCollection;
        }
        else
        {
            propertyTypeCollection = new PropertyTypeCollection(supportsPublishing);
            foreach (var propertyType in _propertyTypeBuilders.Select(x => x.Build()))
            {
                propertyTypeCollection.Add(propertyType);
            }
        }

        return new PropertyGroup(propertyTypeCollection)
        {
            Id = id,
            Key = key,
            Alias = alias,
            Name = name,
            SortOrder = sortOrder,
            CreateDate = createDate,
            UpdateDate = updateDate
        };
    }
}
