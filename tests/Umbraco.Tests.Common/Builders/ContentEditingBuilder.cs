// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentEditingBuilder : ContentEditingBaseBuilder<ContentCreateModel>, IWithTemplateKeyBuilder
{
    private Guid? _templateKey;

    Guid? IWithTemplateKeyBuilder.TemplateKey
    {
        get => _templateKey;
        set => _templateKey = value;
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
                .WithAlias("title")
                .WithValue("Welcome to our Home page")
                .Done()
            .Build();

    public static ContentCreateModel CreateSimpleContent(Guid contentTypeKey, string name, Guid? parentKey) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .WithInvariantName(name)
            .WithParentKey(parentKey)
            .AddInvariantProperty()
                .WithAlias("title")
                .WithValue("Welcome to our Home page")
                .Done()
            .Build();

    public static ContentCreateModel CreateSimpleContent(Guid contentTypeKey, string name) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .WithInvariantName(name)
            .AddInvariantProperty()
                .WithAlias("title")
                .WithValue("Welcome to our Home page")
                .Done()
            .Build();

    public static ContentCreateModel CreateContentWithTwoInvariantProperties(Guid contentTypeKey, string name, string firstPropertyAlias, string firstPropertyValue, string secondPropertyAlias, string secondPropertyValue, Guid? parentKey) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .WithInvariantName(name)
            .WithParentKey(parentKey)
            .AddInvariantProperty()
                .WithAlias(firstPropertyAlias)
                .WithValue(firstPropertyValue)
                .Done()
            .AddInvariantProperty()
                .WithAlias(secondPropertyAlias)
                .WithValue(secondPropertyValue)
                .Done()
            .Build();

    public static ContentCreateModel CreateContentWithOneInvariantProperty(Guid contentTypeKey, string name, string propertyAlias, object propertyValue) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .WithInvariantName(name)
            .AddInvariantProperty()
                .WithAlias(propertyAlias)
                .WithValue(propertyValue)
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
