// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PropertyTypeBuilder : PropertyTypeBuilder<NullPropertyTypeBuilderParent>
{
    public PropertyTypeBuilder()
        : base(null)
    {
    }
}

public class NullPropertyTypeBuilderParent : IBuildPropertyTypes
{
}

public class PropertyTypeBuilder<TParent>
    : ChildBuilderBase<TParent, PropertyType>,
        IWithIdBuilder,
        IWithKeyBuilder,
        IWithAliasBuilder,
        IWithNameBuilder,
        IWithCreateDateBuilder,
        IWithUpdateDateBuilder,
        IWithSortOrderBuilder,
        IWithDescriptionBuilder,
        IWithSupportsPublishing
    where TParent : IBuildPropertyTypes
{
    private string _alias;
    private DateTime? _createDate;
    private int? _dataTypeId;
    private string _description;
    private int? _id;
    private Guid? _key;
    private bool? _labelOnTop;
    private bool? _mandatory;
    private string _mandatoryMessage;
    private string _name;
    private string _propertyEditorAlias;
    private Lazy<int> _propertyGroupId;
    private int? _sortOrder;
    private bool? _supportsPublishing;
    private DateTime? _updateDate;
    private string _validationRegExp;
    private string _validationRegExpMessage;
    private ValueStorageType? _valueStorageType;
    private ContentVariation? _variations;

    public PropertyTypeBuilder(TParent parentBuilder)
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

    string IWithDescriptionBuilder.Description
    {
        get => _description;
        set => _description = value;
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

    public PropertyTypeBuilder<TParent> WithPropertyEditorAlias(string propertyEditorAlias)
    {
        _propertyEditorAlias = propertyEditorAlias;
        return this;
    }

    public PropertyTypeBuilder<TParent> WithValueStorageType(ValueStorageType valueStorageType)
    {
        _valueStorageType = valueStorageType;
        return this;
    }

    public PropertyTypeBuilder<TParent> WithDataTypeId(int dataTypeId)
    {
        _dataTypeId = dataTypeId;
        return this;
    }

    public PropertyTypeBuilder<TParent> WithPropertyGroupId(int propertyGroupId)
    {
        _propertyGroupId = new Lazy<int>(() => propertyGroupId);
        return this;
    }

    public PropertyTypeBuilder<TParent> WithMandatory(bool mandatory, string mandatoryMessage = "")
    {
        _mandatory = mandatory;
        _mandatoryMessage = mandatoryMessage;
        return this;
    }

    public PropertyTypeBuilder<TParent> WithLabelOnTop(bool labelOnTop)
    {
        _labelOnTop = labelOnTop;
        return this;
    }

    public PropertyTypeBuilder<TParent> WithValidationRegExp(string validationRegExp, string validationRegExpMessage = "")
    {
        _validationRegExp = validationRegExp;
        _validationRegExpMessage = validationRegExpMessage;
        return this;
    }

    public PropertyTypeBuilder<TParent> WithVariations(ContentVariation variation)
    {
        _variations = variation;
        return this;
    }

    public override PropertyType Build()
    {
        var id = _id ?? 0;
        var key = _key ?? Guid.NewGuid();
        var propertyEditorAlias = _propertyEditorAlias ?? Constants.PropertyEditors.Aliases.TextBox;
        var valueStorageType = _valueStorageType ?? ValueStorageType.Nvarchar;
        var name = _name ?? Guid.NewGuid().ToString();
        var alias = _alias ?? name.ToCamelCase();
        var createDate = _createDate ?? DateTime.Now;
        var updateDate = _updateDate ?? DateTime.Now;
        var sortOrder = _sortOrder ?? 0;
        var dataTypeId = _dataTypeId ?? -88;
        var description = _description ?? string.Empty;
        var propertyGroupId = _propertyGroupId;
        var mandatory = _mandatory ?? false;
        var mandatoryMessage = _mandatoryMessage ?? string.Empty;
        var validationRegExp = _validationRegExp ?? string.Empty;
        var validationRegExpMessage = _validationRegExpMessage ?? string.Empty;
        var supportsPublishing = _supportsPublishing ?? false;
        var labelOnTop = _labelOnTop ?? false;
        var variations = _variations ?? ContentVariation.Nothing;

        var shortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

        var propertyType = new PropertyType(shortStringHelper, propertyEditorAlias, valueStorageType)
        {
            Id = id,
            Key = key,
            Alias = alias,
            Name = name,
            SortOrder = sortOrder,
            DataTypeId = dataTypeId,
            Description = description,
            CreateDate = createDate,
            UpdateDate = updateDate,
            PropertyGroupId = propertyGroupId,
            Mandatory = mandatory,
            MandatoryMessage = mandatoryMessage,
            ValidationRegExp = validationRegExp,
            ValidationRegExpMessage = validationRegExpMessage,
            SupportsPublishing = supportsPublishing,
            Variations = variations,
            LabelOnTop = labelOnTop
        };

        return propertyType;
    }
}
