// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentEditingBuilder : ContentEditingBaseBuilder<ContentCreateModel>
{
    private Guid? _templateKey;

    public ContentEditingBuilder WithTemplateKey(Guid templateKey)
    {
        _templateKey = templateKey;
        return this;
    }

    public override ContentCreateModel Build()
    {
        _model.TemplateKey = _templateKey;
        return base.Build();
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
