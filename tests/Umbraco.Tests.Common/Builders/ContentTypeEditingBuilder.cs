using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentTypeEditingBuilder : ContentTypeEditingBaseBuilder<ContentTypeEditingBuilder, ContentTypeCreateModel, ContentTypePropertyTypeModel, ContentTypePropertyContainerModel>
{
    private Guid? _key;
    private Guid? _containerKey;
    private IEnumerable<Guid> _allowedTemplateKeys = Array.Empty<Guid>();
    private Guid? _defaultTemplateKey;

    public ContentTypeEditingBuilder WithKey(Guid key)
    {
        _key = key;
        return this;
    }

    public ContentTypeEditingBuilder WithContainerKey(Guid containerKey)
    {
        _containerKey = containerKey;
        return this;
    }

    public ContentTypeEditingBuilder WithDefaultTemplateKey(Guid templateKey)
    {
        _defaultTemplateKey = templateKey;
        return this;
    }

    public ContentTypeEditingBuilder AddAllowedTemplateKeys(IEnumerable<Guid> templateKeys)
    {
        _allowedTemplateKeys = templateKeys;
        return this;
    }

    public override ContentTypeCreateModel Build()
    {
        _model.Key = _key ?? Guid.NewGuid();
        _model.ContainerKey = _containerKey;
        _model.AllowedTemplateKeys = _allowedTemplateKeys;
        _model.DefaultTemplateKey = _defaultTemplateKey;
        base.Build();
        return _model;
    }

    public static ContentTypeCreateModel CreateBasicContentType(string alias = "umbTextpage", string name = "TextPage")
    {
        var builder = new ContentTypeEditingBuilder();
        return (ContentTypeCreateModel)builder
            .WithAlias(alias)
            .WithName(name)
            .Build();
    }

    public static ContentTypeCreateModel CreateSimpleContentType(string alias = "umbTextpage", string name = "TextPage", string propertyGroupName = "Content", Guid? defaultTemplateKey = null)
    {
        var containerKey = Guid.NewGuid();
        var builder = new ContentTypeEditingBuilder();
        return (ContentTypeCreateModel)builder
            .WithAlias(alias)
            .WithName(name)
            .WithAllowAtRoot(true)
            .AddPropertyGroup()
                .WithKey(containerKey)
                .WithName(propertyGroupName)
                .Done()
            .AddPropertyType()
                .WithAlias("title")
                .WithDataTypeKey(Constants.DataTypes.Guids.TextareaGuid)
                .WithName("Title")
                .WithContainerKey(containerKey)
                .Done()
            .WithDefaultTemplateKey(defaultTemplateKey ?? Guid.Empty)
            .AddAllowedTemplateKeys([defaultTemplateKey ?? Guid.Empty])
            .Build();
    }

    public static ContentTypeCreateModel CreateTextPageContentType(string alias = "textPage", string name = "Text Page", Guid defaultTemplateKey = default)
    {
        var containerKeyOne = Guid.NewGuid();
        var containerKeyTwo = Guid.NewGuid();

        var builder = new ContentTypeEditingBuilder();
        return (ContentTypeCreateModel)builder
            .WithAlias(alias)
            .WithName(name)
            .WithAllowAtRoot(true)
            .AddPropertyGroup()
                .WithName("Content")
                .WithKey(containerKeyOne)
                .WithSortOrder(1)
                .Done()
            .AddPropertyType()
                .WithAlias("title")
                .WithName("Title")
                .WithContainerKey(containerKeyOne)
                .WithSortOrder(1)
                .Done()
            .AddPropertyType()
                .WithDataTypeKey(Constants.DataTypes.Guids.RichtextEditorGuid)
                .WithAlias("bodyText")
                .WithName("Body text")
                .WithContainerKey(containerKeyOne)
                .WithSortOrder(2)
                .Done()
            .AddPropertyGroup()
                .WithName("Meta")
                .WithSortOrder(2)
                .WithKey(containerKeyTwo)
                .Done()
            .AddPropertyType()
                .WithAlias("keywords")
                .WithName("Keywords")
                .WithContainerKey(containerKeyTwo)
                .WithSortOrder(1)
                .Done()
            .AddPropertyType()
                .WithAlias("description")
                .WithName("Description")
                .WithContainerKey(containerKeyTwo)
                .WithSortOrder(2)
                .Done()
            .AddAllowedTemplateKeys([defaultTemplateKey])
            .WithDefaultTemplateKey(defaultTemplateKey)
            .Build();
    }

    public static ContentTypeCreateModel CreateElementType(string alias = "textElement", string name = "Text Element")
    {
        var containerKey = Guid.NewGuid();
        var builder = new ContentTypeEditingBuilder();
        return (ContentTypeCreateModel)builder
            .WithAlias(alias)
            .WithName(name)
            .WithIsElement(true)
            .AddPropertyGroup()
                .WithName("Content")
                .WithKey(containerKey)
                .Done()
            .AddPropertyType()
                .WithDataTypeKey(Constants.DataTypes.Guids.RichtextEditorGuid)
                .WithAlias("bodyText")
                .WithName("Body text")
                .WithContainerKey(containerKey)
                .Done()
            .Build();
    }

    public static ContentTypeCreateModel CreateContentTypeWithDataTypeKey(Guid dataTypeKey, string alias = "textElement", string name = "Text Element")
    {
        var containerKey = Guid.NewGuid();
        var builder = new ContentTypeEditingBuilder();
        return (ContentTypeCreateModel)builder
            .WithAlias(alias)
            .WithName(name)
            .WithIsElement(true)
            .AddPropertyGroup()
                .WithName("Content")
                .WithKey(containerKey)
                .Done()
            .AddPropertyType()
                .WithDataTypeKey(dataTypeKey)
                .WithAlias("dataType")
                .WithName("Data Type")
                .WithContainerKey(containerKey)
                .Done()
            .Build();
    }
}
