using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Common.Builders;

public abstract class ContentEditingBaseBuilder<TCreateModel> : BuilderBase<TCreateModel>,
    IWithKeyBuilder,
    IWithContentTypeKeyBuilder,
    IWithParentKeyBuilder,
    IBuildContentTypes
    where TCreateModel : ContentCreationModelBase, new()
{
    protected TCreateModel _model = new();
    private List<ContentEditingPropertyValueBuilder<ContentEditingBaseBuilder<TCreateModel>>> _properties = [];
    private List<ContentEditingVariantBuilder<ContentEditingBaseBuilder<TCreateModel>>> _variants = [];
    private Guid _contentTypeKey;
    private Guid? _parentKey;
    private Guid? _key;

    Guid? IWithKeyBuilder.Key
    {
        get => _key;
        set => _key = value;
    }

    Guid? IWithParentKeyBuilder.ParentKey
    {
        get => _parentKey;
        set => _parentKey = value;
    }

    Guid IWithContentTypeKeyBuilder.ContentTypeKey
    {
        get => _contentTypeKey;
        set => _contentTypeKey = value;
    }

    public ContentEditingVariantBuilder<ContentEditingBaseBuilder<TCreateModel>> AddVariant()
    {
        var builder = new ContentEditingVariantBuilder<ContentEditingBaseBuilder<TCreateModel>>(this);
        _variants.Add(builder);
        return builder;
    }

    public ContentEditingPropertyValueBuilder<ContentEditingBaseBuilder<TCreateModel>> AddProperty()
    {
        var builder = new ContentEditingPropertyValueBuilder<ContentEditingBaseBuilder<TCreateModel>>(this);
        _properties.Add(builder);
        return builder;
    }

    public override TCreateModel Build()
    {
        if (_variants.Any() is false)
        {
            throw new InvalidOperationException("Expected at least one variant (invariant is also a variant).");
        }

        if (_parentKey is not null)
        {
            _model.ParentKey = _parentKey;
        }

        _model.ContentTypeKey = _contentTypeKey;
        _model.Key = _key ?? Guid.NewGuid();
        _model.Properties = _properties.Select(p => p.Build()).ToArray();
        _model.Variants = _variants.Select(x => x.Build()).ToList();

        return _model;
    }
}
