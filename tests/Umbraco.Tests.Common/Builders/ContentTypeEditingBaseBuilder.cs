using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Common.Builders;

public abstract class ContentTypeEditingBaseBuilder<TBuilder, TCreateModel, TPropertyType, TPropertyTypeContainer>
    where TBuilder : ContentTypeEditingBaseBuilder<TBuilder, TCreateModel, TPropertyType, TPropertyTypeContainer>
    where TCreateModel : ContentTypeEditingModelBase<TPropertyType, TPropertyTypeContainer>, new()
    where TPropertyType : PropertyTypeModelBase, new()
    where TPropertyTypeContainer : PropertyTypeContainerModelBase, new()
{
    protected TCreateModel _model = new();
    private string _alias;
    private string _name;
    private string? _description;
    private string _icon;
    private bool? _allowedAsRoot;
    private bool? _isElement;
    private bool? _variesByCulture;
    private bool? _variesBySegment;
    private readonly List<PropertyTypeModelBuilder<TBuilder, TPropertyType>> _propertyTypeBuilders = new();
    private readonly List<PropertyTypeContainerBuilder<TBuilder, TPropertyTypeContainer>> _propertyTypeContainerBuilders = new();
    private readonly List<ContentTypeSortBuilder<TBuilder>> _allowedContentTypeBuilders = new();

    public TBuilder WithAlias(string alias)
    {
        _alias = alias;
        return (TBuilder)this;
    }

    public TBuilder WithName(string name)
    {
        _name = name;
        return (TBuilder)this;
    }

    public TBuilder WithDescription(string? description)
    {
        _description = description;
        return (TBuilder)this;
    }

    public TBuilder WithIcon(string icon)
    {
        _icon = icon;
        return (TBuilder)this;
    }

    public TBuilder WithAllowAtRoot(bool allowAtRoot)
    {
        _allowedAsRoot = allowAtRoot;
        return (TBuilder)this;
    }

    public TBuilder WithIsElement(bool isElement)
    {
        _isElement = isElement;
        return (TBuilder)this;
    }

    public TBuilder WithVariesByCulture(bool variesByCulture)
    {
        _variesByCulture = variesByCulture;
        return (TBuilder)this;
    }

    public TBuilder WithVariesBySegment(bool variesBySegment)
    {
        _variesBySegment = variesBySegment;
        return (TBuilder)this;
    }

    public PropertyTypeModelBuilder<TBuilder, TPropertyType> AddPropertyType()
    {
        var builder = new PropertyTypeModelBuilder<TBuilder, TPropertyType>((TBuilder)this);
        _propertyTypeBuilders.Add(builder);
        return builder;
    }

    public PropertyTypeContainerBuilder<TBuilder, TPropertyTypeContainer> AddPropertyGroup()
    {
        var builder = new PropertyTypeContainerBuilder<TBuilder, TPropertyTypeContainer>((TBuilder)this);
        _propertyTypeContainerBuilders.Add(builder);
        return builder;
    }

    public ContentTypeSortBuilder<TBuilder> AddAllowedContentType()
    {
        var builder = new ContentTypeSortBuilder<TBuilder>((TBuilder)this);
        _allowedContentTypeBuilders.Add(builder);
        return builder;
    }

    public virtual TCreateModel Build()
    {
        _model.Properties = _propertyTypeBuilders.Select(x => x.Build()).Cast<TPropertyType>();
        _model.Containers = _propertyTypeContainerBuilders.Select(x => x.Build()).Cast<TPropertyTypeContainer>();
        _model.AllowedContentTypes = _allowedContentTypeBuilders.Select(x => x.Build());
        _model.Alias = _alias ?? "TestName";
        _model.Name = _name ?? "TestName";
        _model.Description = _description;
        _model.Icon = _icon ?? _model.Icon;
        _model.AllowedAsRoot = _allowedAsRoot ?? false;
        _model.IsElement = _isElement ?? _model.IsElement;
        _model.VariesByCulture = _variesByCulture ?? _model.VariesByCulture;
        _model.VariesBySegment = _variesBySegment ?? _model.VariesBySegment;
        return _model;
    }
}
