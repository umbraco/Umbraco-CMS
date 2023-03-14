// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PropertyBuilder
    : BuilderBase<IProperty>,
        IBuildPropertyTypes,
        IWithIdBuilder,
        IWithKeyBuilder,
        IWithCreateDateBuilder,
        IWithUpdateDateBuilder
{
    private DateTime? _createDate;

    private int? _id;
    private Guid? _key;
    private IPropertyType _propertyType;
    private PropertyTypeBuilder<PropertyBuilder> _propertyTypeBuilder;
    private DateTime? _updateDate;

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

    DateTime? IWithUpdateDateBuilder.UpdateDate
    {
        get => _updateDate;
        set => _updateDate = value;
    }

    public PropertyBuilder WithPropertyType(IPropertyType propertyType)
    {
        _propertyTypeBuilder = null;
        _propertyType = propertyType;
        return this;
    }

    public PropertyTypeBuilder<PropertyBuilder> AddPropertyType()
    {
        _propertyType = null;
        var builder = new PropertyTypeBuilder<PropertyBuilder>(this);
        _propertyTypeBuilder = builder;
        return builder;
    }

    public override IProperty Build()
    {
        var id = _id ?? 1;
        var key = _key ?? Guid.NewGuid();
        var createDate = _createDate ?? DateTime.Now;
        var updateDate = _updateDate ?? DateTime.Now;

        if (_propertyTypeBuilder is null && _propertyType is null)
        {
            throw new InvalidOperationException(
                "A property cannot be constructed without providing a property type. Use AddPropertyType() or WithPropertyType().");
        }

        var propertyType = _propertyType ?? _propertyTypeBuilder.Build();

        // Needs to be within collection to support publishing.
        var propertyTypeCollection = new PropertyTypeCollection(true, new[] { propertyType });

        return new Property(id, propertyTypeCollection[0])
        {
            Key = key,
            CreateDate = createDate,
            UpdateDate = updateDate
        };
    }
}
