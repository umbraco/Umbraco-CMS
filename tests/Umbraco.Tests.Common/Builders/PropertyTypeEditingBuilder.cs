using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PropertyTypeEditingBuilder<TParent, TModel> : ChildBuilderBase<TParent, TModel>, IWithKeyBuilder,
    IWithContainerKeyBuilder, IWithSortOrderBuilder, IWithAliasBuilder, IWithNameBuilder, IWithDescriptionBuilder,
    IWithDataTypeKeyBuilder, IWithVariesByCultureBuilder,
    IWithVariesBySegmentBuilder where TModel : PropertyTypeModelBase, new()
{
    private TModel _model;
    private Guid? _key;
    private Guid? _containerKey;
    private int? _sortOrder;
    private string _alias;
    private string _name;
    private string? _description;
    private Guid? _dataTypeKey;
    private bool _variesByCulture;
    private bool _variesBySegment;
    private PropertyTypeValidationEditingBuilder<TParent, TModel> _validationBuilder;
    private PropertyTypeAppearanceBuilder<TParent, TModel> _appearanceBuilder;

    public PropertyTypeEditingBuilder(TParent parentBuilder)
        : base(parentBuilder)
    {
        _model = new TModel();
        _validationBuilder = new PropertyTypeValidationEditingBuilder<TParent, TModel>(this);
        _appearanceBuilder = new PropertyTypeAppearanceBuilder<TParent, TModel>(this);
    }

    Guid? IWithKeyBuilder.Key
    {
        get => _key;
        set => _key = value;
    }

    Guid? IWithContainerKeyBuilder.ContainerKey
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

    string? IWithDescriptionBuilder.Description
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

    public PropertyTypeValidationEditingBuilder<TParent, TModel> AddValidation()
    {
        var builder = new PropertyTypeValidationEditingBuilder<TParent, TModel>(this);
        _validationBuilder = builder;
        return builder;
    }

    public PropertyTypeAppearanceBuilder<TParent, TModel> AddAppearance()
    {
        var builder = new PropertyTypeAppearanceBuilder<TParent, TModel>(this);
        _appearanceBuilder = builder;
        return builder;
    }

    public override TModel Build()
    {
        _model.Key = _key ?? Guid.NewGuid();
        _model.ContainerKey = _containerKey;
        _model.SortOrder = _sortOrder ?? 0;
        _model.Alias = _alias ?? "title";
        _model.Name = _name ?? "Title";
        _model.Description = _description;
        _model.DataTypeKey = _dataTypeKey ?? Constants.DataTypes.Guids.TextareaGuid;
        _model.VariesByCulture = _variesByCulture;
        _model.VariesBySegment = _variesBySegment;
        _model.Validation = _validationBuilder.Build();
        _model.Appearance = _appearanceBuilder.Build();

        return _model;
    }
}
