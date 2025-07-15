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
            .AddVariant()
                .WithName("Home")
                .Done()
            .Build();

    public static ContentCreateModel CreateSimpleContent(Guid contentTypeKey) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .AddVariant()
                .WithName("Home")
                .Done()
            .AddProperty()
                .WithAlias("title")
                .WithValue("Welcome to our Home page")
                .Done()
            .Build();

    public static ContentCreateModel CreateSimpleContent(Guid contentTypeKey, string name, Guid? parentKey) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .AddVariant()
                .WithName(name)
                .Done()
            .WithParentKey(parentKey)
            .AddProperty()
                .WithAlias("title")
                .WithValue("Welcome to our Home page")
                .Done()
            .Build();

    public static ContentCreateModel CreateSimpleContent(Guid contentTypeKey, string name) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .AddVariant()
                .WithName(name)
                .Done()
            .AddProperty()
                .WithAlias("title")
                .WithValue("Welcome to our Home page")
                .Done()
            .Build();

    public static ContentCreateModel CreateContentWithTwoInvariantProperties(Guid contentTypeKey, string name, string firstPropertyAlias, string firstPropertyValue, string secondPropertyAlias, string secondPropertyValue, Guid? parentKey) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .AddVariant()
                .WithName(name)
                .Done()
            .WithParentKey(parentKey)
            .AddProperty()
                .WithAlias(firstPropertyAlias)
                .WithValue(firstPropertyValue)
                .Done()
            .AddProperty()
                .WithAlias(secondPropertyAlias)
                .WithValue(secondPropertyValue)
                .Done()
            .Build();

    public static ContentCreateModel CreateContentWithOneInvariantProperty(Guid contentTypeKey, string name, string propertyAlias, object propertyValue) =>
        new ContentEditingBuilder()
            .WithContentTypeKey(contentTypeKey)
            .AddVariant()
                .WithName(name)
                .Done()
            .AddProperty()
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
                .Done()
            .AddVariant()
                .WithCulture(secondCulture)
                .WithName(secondCulture)
                .Done()
            .AddProperty()
                .WithCulture(firstCulture)
                .WithAlias(propertyAlias)
                .WithValue(propertyName)
                .Done()
            .AddProperty()
                .WithCulture(secondCulture)
                .WithAlias(propertyAlias)
                .WithValue(propertyName)
                .Done()
            .Build();
}
