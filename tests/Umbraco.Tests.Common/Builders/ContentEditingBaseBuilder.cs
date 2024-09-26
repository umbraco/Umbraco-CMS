using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;
using Umbraco.Cms.Tests.Common.Builders.Interfaces.ContentCreateModel;

namespace Umbraco.Cms.Tests.Common.Builders;

public abstract class ContentEditingBaseBuilder<TCreateModel> : BuilderBase<TCreateModel>,
    IWithInvariantNameBuilder,
    IWithInvariantPropertiesBuilder,
    IWithVariantsBuilder,
    IWithKeyBuilder,
    IWithContentTypeKeyBuilder,
    IWithParentKeyBuilder,
    IBuildContentTypes
    where TCreateModel : ContentCreationModelBase, new()
{
    protected TCreateModel _model = new();
    private IEnumerable<PropertyValueModel> _invariantProperties = [];
    private IEnumerable<VariantModel> _variants = [];
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

    public IEnumerable<PropertyValueModel> InvariantProperties
    {
        get => _invariantProperties;
        set => _invariantProperties = value;
    }

    public IEnumerable<VariantModel> Variants
    {
        get => _variants;
        set => _variants = value;
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

    public ContentEditingBaseBuilder<TCreateModel> WithInvariantName(string invariantName)
    {
        _invariantName = invariantName;
        return this;
    }

    public ContentEditingBaseBuilder<TCreateModel> WithInvariantProperty(string alias, object value)
    {
        var property = new PropertyValueModel { Alias = alias, Value = value };
        _invariantProperties = _invariantProperties.Concat(new[] { property });
        return this;
    }

    public ContentEditingBaseBuilder<TCreateModel> AddVariant(string culture, string segment, string name, IEnumerable<PropertyValueModel> properties)
    {
        var variant = new VariantModel { Culture = culture, Segment = segment, Name = name, Properties = properties };
        _variants = _variants.Concat(new[] { variant });
        return this;
    }

    public ContentEditingBaseBuilder<TCreateModel> WithParentKey(Guid parentKey)
    {
        _parentKey = parentKey;
        return this;
    }

    public ContentEditingBaseBuilder<TCreateModel> WithContentTypeKey(Guid contentTypeKey)
    {
        _contentTypeKey = contentTypeKey;
        return this;
    }

    public ContentEditingBaseBuilder<TCreateModel> WithKey(Guid key)
    {
        _key = key;
        return this;
    }

    public override TCreateModel Build()
    {
        if (_parentKey is not null)
        {
            _model.ParentKey = _parentKey;
        }

        _model.InvariantName = _invariantName ?? Guid.NewGuid().ToString();
        _model.ContentTypeKey = _contentTypeKey;
        _model.Key = _key ?? Guid.NewGuid();
        _model.InvariantProperties = _invariantProperties;
        _model.Variants = _variants;

        return _model;
    }
}
