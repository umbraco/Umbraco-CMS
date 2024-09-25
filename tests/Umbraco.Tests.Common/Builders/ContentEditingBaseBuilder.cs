using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;
using Umbraco.Cms.Tests.Common.Builders.Interfaces.ContentCreateModel;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentEditingBaseBuilder<TCreateModel> : BuilderBase<TCreateModel>,
    IWithInvariantNameBuilder,
    IWithInvariantPropertiesBuilder,
    IWithVariantsBuilder,
    IWithKeyBuilder,
    IWithContentTypeKeyBuilder,
    IWithParentKeyBuilder,
    IBuildContentTypes
    where TCreateModel : ContentCreationModelBase, new()
{
    protected TCreateModel _model = new TCreateModel();
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

    public ContentEditingBaseBuilder<TCreateModel> AddVariant(string culture, string segment, string name,
        IEnumerable<PropertyValueModel> properties)
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

    public override TCreateModel Build()
    {
        var key = _key ?? Guid.NewGuid();
        var parentKey = _parentKey;
        var invariantName = _invariantName ?? Guid.NewGuid().ToString();
        var invariantProperties = _invariantProperties;
        var variants = _variants;
        var contentTypeKey = _contentTypeKey;

        if (parentKey is not null)
        {
            _model.ParentKey = parentKey;
        }

        _model.InvariantName = invariantName;
        _model.ContentTypeKey = contentTypeKey;
        _model.Key = key;
        _model.InvariantProperties = invariantProperties;
        _model.Variants = variants;

        return _model;
    }
}
