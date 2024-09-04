using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentTypeEditingBuilder
    : ContentTypeBaseBuilder<ContentEditingBuilder, IContentType>,
        IBuildPropertyTypes
{
    private Guid? _key;
    private Guid? _containerKey;
    private ContentTypeCleanup _cleanup;
    private IEnumerable<Guid> _allowedTemplateKeys;
    private Guid? _defaultTemplateKey;
    private bool? _isElement;
    private ContentVariation? _contentVariation;
    private readonly List<PropertyTypeBuilder<ContentTypeEditingBuilder>> _groupPropertyTypeBuilders = new();
    private readonly List<PropertyGroupBuilder<ContentTypeEditingBuilder>> _propertyGroupBuilders = new();
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

    public PropertyGroupBuilder<ContentTypeEditingBuilder> AddPropertyGroup()
    {
        var builder = new PropertyGroupBuilder<ContentTypeEditingBuilder>(this);
        _propertyGroupBuilders.Add(builder);
        return builder;
    }

    public PropertyTypeBuilder<ContentTypeEditingBuilder> AddPropertyType()
    {
        var builder = new PropertyTypeBuilder<ContentTypeEditingBuilder>(this);
        _groupPropertyTypeBuilders.Add(builder);
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

    public override IContentType Build()
    {
        var contentVariation = _contentVariation ?? ContentVariation.Nothing;

        ContentType contentType;
        return contentType
    }
}
