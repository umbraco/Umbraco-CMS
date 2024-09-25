using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PropertyTypeEditingBuilder<TParent>
    : ChildBuilderBase<TParent, ContentTypePropertyTypeModel>, IBuildPropertyTypes, IWithKeyBuilder,
        IWIthContainerKeyBuilder,
        IWithSortOrderBuilder, IWithAliasBuilder, IWithNameBuilder, IWithDescriptionBuilder, IWithDataTypeKeyBuilder,
        IWithVariesByCultureBuilder, IWithVariesBySegmentBuilder
{
    private Guid? _key;
    private Guid? _containerKey;
    private int? _sortOrder;
    private string _alias;
    private string? _name;
    private string? _description;
    private Guid? _dataTypeKey;
    private bool _variesByCulture;
    private bool _variesBySegment;
    private PropertyTypeValidationEditingBuilder<TParent> _validationBuilder;
    private PropertyTypeAppearanceBuilder<TParent> _appearanceBuilder;

    public PropertyTypeEditingBuilder(TParent parentBuilder) : base(parentBuilder)
    {
        _validationBuilder = new PropertyTypeValidationEditingBuilder<TParent>(this);
        _appearanceBuilder = new PropertyTypeAppearanceBuilder<TParent>(this);
    }


    Guid? IWithKeyBuilder.Key
    {
        get => _key;
        set => _key = value;
    }

    Guid? IWIthContainerKeyBuilder.ContainerKey
    {
        get => _containerKey;
        set => _containerKey = value;
    }

    int? IWithSortOrderBuilder.SortOrder
    {
        get => _sortOrder;
        set => _sortOrder = value;
    }

    string IWithAliasBuilder.Alias
    {
        get => _alias;
        set => _alias = value;
    }

    string IWithNameBuilder.Name
    {
        get => _name;
        set => _name = value;
    }

    string IWithDescriptionBuilder.Description
    {
        get => _description;
        set => _description = value;
    }

    Guid? IWithDataTypeKeyBuilder.DataTypeKey
    {
        get => _dataTypeKey;
        set => _dataTypeKey = value;
    }

    bool IWithVariesByCultureBuilder.VariesByCulture
    {
        get => _variesByCulture;
        set => _variesByCulture = value;
    }

    bool IWithVariesBySegmentBuilder.VariesBySegment
    {
        get => _variesBySegment;
        set => _variesBySegment = value;
    }

    public PropertyTypeValidationEditingBuilder<TParent> AddValidation()
    {
        var builder = new PropertyTypeValidationEditingBuilder<TParent>(this);
        _validationBuilder = builder;
        return builder;
    }

    public PropertyTypeAppearanceBuilder<TParent> AddAppearance()
    {
        var builder = new PropertyTypeAppearanceBuilder<TParent>(this);
        _appearanceBuilder = builder;
        return builder;
    }

    public PropertyTypeEditingBuilder<TParent> WithContainerKey(Guid? containerKey)
    {
        _containerKey = containerKey;
        return this;
    }

    public PropertyTypeEditingBuilder<TParent> WithSortOrder(int sortOrder)
    {
        _sortOrder = sortOrder;
        return this;
    }

    public PropertyTypeEditingBuilder<TParent> WithAlias(string alias)
    {
        _alias = alias;
        return this;
    }

    public PropertyTypeEditingBuilder<TParent> WithName(string name)
    {
        _name = name;
        return this;
    }

    public PropertyTypeEditingBuilder<TParent> WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public PropertyTypeEditingBuilder<TParent> WithDataTypeKey(Guid dataTypeKey)
    {
        _dataTypeKey = dataTypeKey;
        return this;
    }

    public PropertyTypeEditingBuilder<TParent> WithVariesByCulture(bool variesByCulture)
    {
        _variesByCulture = variesByCulture;
        return this;
    }

    public PropertyTypeEditingBuilder<TParent> WithVariesBySegment(bool variesBySegment)
    {
        _variesBySegment = variesBySegment;
        return this;
    }

    public override ContentTypePropertyTypeModel Build()
    {
        var key = _key ?? Guid.NewGuid();
        var containerKey = _containerKey;
        var sortOrder = _sortOrder ?? 0;
        var alias = _alias ?? "title";
        var name = _name ?? "Title";
        var description = _description;
        var dataTypeKey = _dataTypeKey ?? Constants.DataTypes.Guids.TextareaGuid;
        var variesByCulture = _variesByCulture;
        var variesBySegment = _variesBySegment;
        var validation = _validationBuilder.Build();
        var appearance = _appearanceBuilder.Build();

        return new ContentTypePropertyTypeModel
        {
            Key = key,
            ContainerKey = containerKey,
            SortOrder = sortOrder,
            Alias = alias,
            Name = name,
            Description = description,
            DataTypeKey = dataTypeKey,
            VariesByCulture = variesByCulture,
            VariesBySegment = variesBySegment,
            Validation = validation,
            Appearance = appearance,
        };
    }
}
