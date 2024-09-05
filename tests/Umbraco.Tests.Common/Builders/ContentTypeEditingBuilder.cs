using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentTypeEditingBuilder
    : ContentTypeBaseBuilder<ContentEditingBuilder, ContentTypeCreateModel>,
        IBuildPropertyTypes
{
    private Guid? _key;
    private Guid? _containerKey;
    private ContentTypeCleanup _cleanup;
    private IEnumerable<Guid> _allowedTemplateKeys;
    private Guid? _defaultTemplateKey;
    private bool? _allowAtRoot;
    private bool? _isElement;
    private bool? _variesByCulture;
    private bool? _variesBySegment;
    private readonly List<PropertyTypeEditingBuilder<ContentTypeEditingBuilder>> _propertyTypeBuilders = new();

    private readonly List<PropertyTypeContainerBuilder<ContentTypeEditingBuilder>> _propertyTypeContainerBuilders =
        new();

    private readonly List<ContentTypeSortBuilder> _allowedContentTypeBuilders = new();
    private readonly List<TemplateBuilder> _templateBuilders = new();


    public ContentTypeEditingBuilder()
        : base(null)
    {
    }


    public ContentTypeEditingBuilder(ContentEditingBuilder parentBuilder)
        : base(parentBuilder)
    {
    }

    public ContentTypeEditingBuilder WithDefaultTemplateKey(Guid templateKey)
    {
        _defaultTemplateKey = templateKey;
        return this;
    }

    public ContentTypeEditingBuilder WithIsElement(bool isElement)
    {
        _isElement = isElement;
        return this;
    }

    public PropertyTypeContainerBuilder<ContentTypeEditingBuilder> AddPropertyGroup()
    {
        var builder = new PropertyTypeContainerBuilder<ContentTypeEditingBuilder>(this);
        _propertyTypeContainerBuilders.Add(builder);
        return builder;
    }

    public PropertyTypeEditingBuilder<ContentTypeEditingBuilder> AddPropertyType()
    {
        var builder = new PropertyTypeEditingBuilder<ContentTypeEditingBuilder>(this);
        _propertyTypeBuilders.Add(builder);
        return builder;
    }

    public TemplateBuilder AddAllowedTemplate()
    {
        var builder = new TemplateBuilder(this);
        _templateBuilders.Add(builder);
        return builder;
    }

    public ContentTypeSortBuilder AddAllowedContentType()
    {
        var builder = new ContentTypeSortBuilder(this);
        _allowedContentTypeBuilders.Add(builder);
        return builder;
    }

    public override ContentTypeCreateModel Build()
    {
        ContentTypeCreateModel contentType = new ContentTypeCreateModel();
        contentType.Name = GetName();
        contentType.Alias = GetAlias();
        contentType.Key = GetKey();
        contentType.ContainerKey = _containerKey;
        contentType.Cleanup = _cleanup;
        contentType.AllowedTemplateKeys = _allowedTemplateKeys;
        contentType.DefaultTemplateKey = _defaultTemplateKey;
        contentType.IsElement = _isElement ?? false;
        contentType.VariesByCulture = _variesByCulture ?? false;
        contentType.VariesBySegment = _variesBySegment ?? false;
        contentType.AllowedAsRoot = _allowAtRoot ?? false;
        contentType.Properties = _propertyTypeBuilders.Select(x => x.Build()).ToList();
        contentType.Containers = _propertyTypeContainerBuilders.Select(x => x.Build()).ToList();
        contentType.AllowedContentTypes = _allowedContentTypeBuilders.Select(x => x.Build()).ToList();

        return contentType;
    }

    public static ContentTypeCreateModel CreateBasicContentType(string alias = "basePage", string name = "Base Page",
        IContentType parent = null)
    {
        var builder = new ContentTypeEditingBuilder();
        return (ContentTypeCreateModel)builder
            .WithAlias(alias)
            .WithName(name)
            .WithParentContentType(parent)
            .Build();
    }

    public static ContentTypeCreateModel CreateSimpleContentType(string alias, string name, IContentType parent = null, string propertyGroupName = "Content", Guid? defaultTemplateKey = null)
    {
        var containerKey = Guid.NewGuid();
        var builder = new ContentTypeEditingBuilder();
        return (ContentTypeCreateModel)builder
            .WithAlias(alias)
            .WithName(name)
            .WithParentContentType(parent)
            .AddPropertyGroup()
            .WithKey(containerKey)
            .WithName(propertyGroupName)
            .Done()
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeKey(Constants.DataTypes.Guids.TextareaGuid)
            .WithName("Title")
            .Done()
            .WithDefaultTemplateKey(defaultTemplateKey ?? Guid.Empty)
            .Build();
    }
}
