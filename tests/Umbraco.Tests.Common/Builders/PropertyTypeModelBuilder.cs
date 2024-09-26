using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PropertyTypeModelBuilder<TParent, TModel> : ChildBuilderBase<TParent, TModel>
    where TModel : PropertyTypeModelBase, new()
{
    private readonly TModel _model;
    private Guid? _key;
    private Guid? _containerKey;
    private int? _sortOrder;
    private string _alias;
    private string? _name;
    private string? _description;
    private Guid? _dataTypeKey;
    private bool _variesByCulture;
    private bool _variesBySegment;
    private PropertyTypeValidationEditingBuilder<TParent, TModel> _validationBuilder;
    private PropertyTypeAppearanceBuilder<TParent, TModel> _appearanceBuilder;

    public PropertyTypeModelBuilder(TParent parentBuilder)
        : base(parentBuilder)
    {
        _model = new TModel();
        _validationBuilder = new PropertyTypeValidationEditingBuilder<TParent, TModel>(this);
        _appearanceBuilder = new PropertyTypeAppearanceBuilder<TParent, TModel>(this);
    }

    public PropertyTypeModelBuilder<TParent, TModel> WithKey(Guid key)
    {
        _key = key;
        return this;
    }

    public PropertyTypeModelBuilder<TParent, TModel> WithContainerKey(Guid? containerKey)
    {
        _containerKey = containerKey;
        return this;
    }

    public PropertyTypeModelBuilder<TParent, TModel> WithSortOrder(int sortOrder)
    {
        _sortOrder = sortOrder;
        return this;
    }

    public PropertyTypeModelBuilder<TParent, TModel> WithAlias(string alias)
    {
        _alias = alias;
        return this;
    }

    public PropertyTypeModelBuilder<TParent, TModel> WithName(string name)
    {
        _name = name;
        return this;
    }

    public PropertyTypeModelBuilder<TParent, TModel> WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public PropertyTypeModelBuilder<TParent, TModel> WithDataTypeKey(Guid dataTypeKey)
    {
        _dataTypeKey = dataTypeKey;
        return this;
    }

    public PropertyTypeModelBuilder<TParent, TModel> WithVariesByCulture(bool variesByCulture)
    {
        _variesByCulture = variesByCulture;
        return this;
    }

    public PropertyTypeModelBuilder<TParent, TModel> WithVariesBySegment(bool variesBySegment)
    {
        _variesBySegment = variesBySegment;
        return this;
    }

    public PropertyTypeModelBuilder<TParent, TModel> WithValidation(PropertyTypeValidationEditingBuilder<TParent, TModel> validation)
    {
        _validationBuilder = validation;
        return this;
    }

    public PropertyTypeModelBuilder<TParent, TModel> WithAppearance(PropertyTypeAppearanceBuilder<TParent, TModel> appearance)
    {
        _appearanceBuilder = appearance;
        return this;
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
