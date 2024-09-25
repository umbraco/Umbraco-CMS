// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;
using Umbraco.Cms.Tests.Common.Builders.Interfaces.ContentCreateModel;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentEditingBuilder
    : ContentEditingBaseBuilder<ContentCreateModel>,
        IWithInvariantNameBuilder,
        IWithInvariantPropertiesBuilder,
        IWithVariantsBuilder,
        IWithKeyBuilder,
        IWithContentTypeKeyBuilder,
        IWithParentKeyBuilder,
        IWithTemplateKeyBuilder
{
    private ContentTypeEditingBuilder _contentTypeEditingBuilder;
    private IEnumerable<PropertyValueModel> _invariantProperties = [];
    private IEnumerable<VariantModel> _variants = [];
    private Guid _contentTypeKey;
    private Guid? _parentKey;
    private Guid? _templateKey;
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

    IEnumerable<PropertyValueModel> IWithInvariantPropertiesBuilder.InvariantProperties
    {
        get => _invariantProperties;
        set => _invariantProperties = value;
    }

    IEnumerable<VariantModel> IWithVariantsBuilder.Variants
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

    Guid? IWithTemplateKeyBuilder.TemplateKey
    {
        get => _templateKey;
        set => _templateKey = value;
    }

    public ContentEditingBuilder WithInvariantName(string invariantName)
    {
        _invariantName = invariantName;
        return this;
    }

    public ContentEditingBuilder WithInvariantProperty(string alias, object value)
    {
        var property = new PropertyValueModel { Alias = alias, Value = value };
        _invariantProperties = _invariantProperties.Concat(new[] { property });
        return this;
    }

    public ContentEditingBuilder AddVariant(string culture, string segment, string name, IEnumerable<PropertyValueModel> properties)
    {
        var variant = new VariantModel { Culture = culture, Segment = segment, Name = name, Properties = properties };
        _variants = _variants.Concat(new[] { variant });
        return this;
    }

    public ContentEditingBuilder WithParentKey(Guid parentKey)
    {
        _parentKey = parentKey;
        return this;
    }

    public ContentEditingBuilder WithTemplateKey(Guid templateKey)
    {
        _templateKey = templateKey;
        return this;
    }

    public override ContentCreateModel Build()
    {
        var key = _key ?? Guid.NewGuid();
        var parentKey = _parentKey;
        var templateKey = _templateKey;
        var invariantName = _invariantName ?? Guid.NewGuid().ToString();
        var invariantProperties = _invariantProperties;
        var variants = _variants;
        var contentTypeKey = _contentTypeKey;

        var content = new ContentCreateModel();
        if (parentKey is not null)
        {
            content.ParentKey = parentKey;
        }

        if (templateKey is not null)
        {
            content.TemplateKey = templateKey;
        }

        content.InvariantName = invariantName;
        content.ContentTypeKey = contentTypeKey;
        content.Key = key;
        content.InvariantProperties = invariantProperties;
        content.Variants = variants;

        return content;
    }

    public static ContentCreateModel CreateBasicContent(Guid contentTypeKey, Guid? key) =>
        new ContentEditingBuilder()
            .WithKey(key)
            .WithContentTypeKey(contentTypeKey)
            .WithInvariantName("Home")
            .Build();

    public static ContentCreateModel CreateSimpleContent(Guid contentTypeKey) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .WithInvariantName("Home")
            .WithInvariantProperty("title", "Welcome to our Home page")
            .Build();

    public static ContentCreateModel CreateSimpleContent(Guid contentTypeKey, string name, Guid? parentKey) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .WithInvariantName(name)
            .WithParentKey(parentKey)
            .WithInvariantProperty("title", "Welcome to our Home page")
            .Build();

    public static ContentCreateModel CreateSimpleContent(Guid contentTypeKey, string name) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .WithInvariantName(name)
            .WithInvariantProperty("title", "Welcome to our Home page")
            .Build();

    public static ContentCreateModel CreateContentWithTwoVariantProperties(Guid contentTypeKey, string firstCulture, string secondCulture, string propertyAlias, string propertyName) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .AddVariant(firstCulture, null, firstCulture, new[] { new PropertyValueModel { Alias = propertyAlias, Value = propertyName } })
            .AddVariant(secondCulture, null, secondCulture, new[] { new PropertyValueModel { Alias = propertyAlias, Value = propertyName } })
            .Build();
}
