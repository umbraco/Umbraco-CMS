using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public abstract class ContentTypeEditingBaseBuilder<TBuilder, TCreateModel, TPropertyType, TPropertyTypeContainer> :
    IWithAliasBuilder, IWithNameBuilder, IWithDescriptionBuilder, IWithIconBuilder, IWithAllowAsRootBuilder,
    IWithIsElementBuilder, IWithVariesByCultureBuilder,
    IWithVariesBySegmentBuilder
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
    private readonly List<PropertyTypeEditingBuilder<TBuilder, TPropertyType>> _propertyTypeBuilders = new();
    private readonly List<PropertyTypeContainerBuilder<TBuilder, TPropertyTypeContainer>> _propertyTypeContainerBuilders = new();
    private readonly List<ContentTypeSortBuilder<TBuilder>> _allowedContentTypeBuilders = new();

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

    string IWithIconBuilder.Icon
    {
        get => _icon;
        set => _icon = value;
    }

    bool? IWithAllowAsRootBuilder.AllowAsRoot
    {
        get => _allowedAsRoot ?? false;
        set => _allowedAsRoot = value;
    }

    bool? IWithIsElementBuilder.IsElement
    {
        get => _isElement ?? false;
        set => _isElement = value;
    }


    bool IWithVariesByCultureBuilder.VariesByCulture
    {
        get => _variesByCulture ?? false;
        set => _variesByCulture = value;
    }

    bool IWithVariesBySegmentBuilder.VariesBySegment
    {
        get => _variesBySegment ?? false;
        set => _variesBySegment = value;
    }

    public PropertyTypeEditingBuilder<TBuilder, TPropertyType> AddPropertyType()
    {
        var builder = new PropertyTypeEditingBuilder<TBuilder, TPropertyType>((TBuilder)this);
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

    protected virtual TCreateModel Build()
    {
        _model.Properties = _propertyTypeBuilders.Select(x => x.Build());
        _model.Containers = _propertyTypeContainerBuilders.Select(x => x.Build());
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
