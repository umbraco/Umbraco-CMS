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
            .AddInvariantProperty()
                .WithAlias("alias")
                .WithValue("Welcome to our Home page")
                .Done()
            .Build();

    public static ContentCreateModel CreateSimpleContent(Guid contentTypeKey, string name, Guid? parentKey) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .WithInvariantName(name)
            .WithParentKey(parentKey)
            .AddInvariantProperty()
                .WithAlias("alias")
                .WithValue("Welcome to our Home page")
                .Done()
            .Build();

    public static ContentCreateModel CreateSimpleContent(Guid contentTypeKey, string name) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .WithInvariantName(name)
            .AddInvariantProperty()
                .WithAlias("alias")
                .WithValue("Welcome to our Home page")
                .Done()
            .Build();

    public static ContentCreateModel CreateContentWithTwoVariantProperties(Guid contentTypeKey, string firstCulture, string secondCulture, string propertyAlias, string propertyName) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .AddVariant()
                .WithCulture(firstCulture)
                .WithName(firstCulture)
                .AddProperty()
                    .WithAlias(propertyAlias)
                    .WithValue(propertyName)
                    .Done()
                .Done()
            .AddVariant()
                .WithCulture(secondCulture)
                .WithName(secondCulture)
                .AddProperty()
                    .WithAlias(propertyAlias)
                    .WithValue(propertyName)
                    .Done()
                .Done()
            .Build();
}
