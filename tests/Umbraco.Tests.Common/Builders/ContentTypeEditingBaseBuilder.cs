using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

public abstract class ContentTypeEditingBuilderBase<TBuilder, TCreateModel, TPropertyType, TPropertyTypeContainer>
    where TBuilder : ContentTypeEditingBuilderBase<TBuilder, TCreateModel, TPropertyType, TPropertyTypeContainer>
    where TCreateModel : ContentTypeEditingModelBase<TPropertyType, TPropertyTypeContainer>, new()
    where TPropertyType : PropertyTypeModelBase
    where TPropertyTypeContainer : PropertyTypeContainerModelBase
{
    protected TCreateModel _model = new TCreateModel();
    private readonly List<PropertyTypeEditingBuilder<TBuilder>> _propertyTypeBuilders = new();
    private readonly List<PropertyTypeContainerBuilder<TBuilder>> _propertyTypeContainerBuilders = new();
    private readonly List<ContentTypeSortBuilder<TBuilder>> _allowedContentTypeBuilders = new();

    // Fluent methods for common propertiess
    public TBuilder WithAlias(string alias)
    {
        _model.Alias = alias;
        return (TBuilder)this;
    }

    public TBuilder WithName(string name)
    {
        _model.Name = name;
        return (TBuilder)this;
    }

    public TBuilder WithDescription(string? description)
    {
        _model.Description = description;
        return (TBuilder)this;
    }

    public TBuilder WithIcon(string icon)
    {
        _model.Icon = icon;
        return (TBuilder)this;
    }

    public TBuilder WithAllowAtRoot(bool allowAtRoot)
    {
        _model.AllowedAsRoot = allowAtRoot;
        return (TBuilder)this;
    }

    public TBuilder WithIsElement(bool isElement)
    {
        _model.IsElement = isElement;
        return (TBuilder)this;
    }

    public TBuilder WithVariesByCulture(bool variesByCulture)
    {
        _model.VariesByCulture = variesByCulture;
        return (TBuilder)this;
    }

    public TBuilder WithVariesBySegment(bool variesBySegment)
    {
        _model.VariesBySegment = variesBySegment;
        return (TBuilder)this;
    }

    public PropertyTypeEditingBuilder<TBuilder> AddPropertyType()
    {
        var builder = new PropertyTypeEditingBuilder<TBuilder>((TBuilder)this);
        _propertyTypeBuilders.Add(builder);
        return builder;
    }

    public PropertyTypeContainerBuilder<TBuilder> AddPropertyGroup()
    {
        var builder = new PropertyTypeContainerBuilder<TBuilder>((TBuilder)this);
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
        _model.Properties = (IEnumerable<TPropertyType>)_propertyTypeBuilders.Select(x => x.Build());
        _model.Containers = (IEnumerable<TPropertyTypeContainer>)_propertyTypeContainerBuilders.Select(x => x.Build());
        _model.AllowedContentTypes = _allowedContentTypeBuilders.Select(x => x.Build());

        return _model;
    }
}
