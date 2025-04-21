using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Common.Builders;

public abstract class ContentEditingBaseBuilder<TCreateModel> : BuilderBase<TCreateModel>,
    IWithInvariantNameBuilder,
    IWithKeyBuilder,
    IWithContentTypeKeyBuilder,
    IWithParentKeyBuilder,
    IBuildContentTypes
    where TCreateModel : ContentCreationModelBase, new()
{
    protected TCreateModel _model = new();
    private List<ContentEditingPropertyValueBuilder<ContentEditingBaseBuilder<TCreateModel>>> _invariantProperties = [];
    private List<ContentEditingVariantBuilder<ContentEditingBaseBuilder<TCreateModel>>> _variants = [];
    private Guid _contentTypeKey;
    private Guid? _parentKey;
    private Guid? _key;
    private string _invariantName;

    Guid? IWithKeyBuilder.Key
    {
        get => _key;
        set => _key = value;
    }

    string IWithInvariantNameBuilder.InvariantName
    {
        get => _invariantName;
        set => _invariantName = value;
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

    public ContentEditingPropertyValueBuilder<ContentEditingBaseBuilder<TCreateModel>> AddInvariantProperty()
    {
        var builder = new ContentEditingPropertyValueBuilder<ContentEditingBaseBuilder<TCreateModel>>(this);
        _invariantProperties.Add(builder);
        return builder;
    }

    public ContentEditingVariantBuilder<ContentEditingBaseBuilder<TCreateModel>> AddVariant()
    {
        var builder = new ContentEditingVariantBuilder<ContentEditingBaseBuilder<TCreateModel>>(this);
        _variants.Add(builder);
        return builder;
    }

    public override TCreateModel Build()
    {
        if (_parentKey is not null)
        {
            _model.ParentKey = _parentKey;
        }

        _model.ContentTypeKey = _contentTypeKey;
        _model.Key = _key ?? Guid.NewGuid();
        _model.Properties = _invariantProperties
            .Select(p => p.Build())
            .Union(_variants.SelectMany(variant => variant.GetProperties().Select(p => p.Build())))
            .ToArray();

        if (_invariantName.IsNullOrWhiteSpace() is false)
        {
            if (_variants.Any())
            {
                throw new InvalidOperationException("Cannot combine invariant and variant variants.");
            }

            AddVariant().WithName(_invariantName);
        }

        _model.Variants = _variants.Select(x => x.Build()).ToList();

        return _model;
    }
}
