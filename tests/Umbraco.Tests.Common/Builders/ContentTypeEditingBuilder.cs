using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentTypeEditingBuilder
    : ContentTypeEditingBuilderBase<ContentTypeEditingBuilder, ContentTypeCreateModel, ContentTypePropertyTypeModel, ContentTypePropertyContainerModel>,
        IBuildPropertyTypes
{
    private Guid? _key;
    private Guid? _containerKey;
    private ContentTypeCleanup _cleanup = new();
    private IEnumerable<Guid> _allowedTemplateKeys;
    private Guid? _defaultTemplateKey;
    private bool? _allowAtRoot;
    private bool? _isElement;
    private bool? _variesByCulture;
    private bool? _variesBySegment;
    private readonly List<PropertyTypeEditingBuilder<ContentTypeEditingBuilder>> _propertyTypeBuilders = [];
    private readonly List<PropertyTypeContainerBuilder<ContentTypeEditingBuilder>> _propertyTypeContainerBuilders = [];
    private readonly List<ContentTypeSortBuilder<ContentTypeEditingBuilder>> _allowedContentTypeBuilders = [];

    public ContentTypeEditingBuilder()
        : base(null)
    {
    }


    public ContentTypeEditingBuilder WithDefaultTemplateKey(Guid templateKey)
    {
        _defaultTemplateKey = templateKey;
        return this;
    }

    public ContentTypeEditingBuilder WithIsElement(bool isElement)
    {
        _isElement = isElement;
        return this;
    }

    public ContentTypeEditingBuilder AddAllowedTemplateKeys(IEnumerable<Guid> templateKeys)
    {
        _allowedTemplateKeys = templateKeys;
        return this;
    }

    public ContentTypeEditingBuilder WithAllowAtRoot(bool allowAtRoot)
    {
        _allowAtRoot = allowAtRoot;
        return this;
    }

    public ContentTypeEditingBuilder WithVariesByCulture(bool variesByCulture)
    {
        _variesByCulture = variesByCulture;
        return this;
    }

    public ContentTypeEditingBuilder WithVariesBySegment(bool variesBySegment)
    {
        _variesBySegment = variesBySegment;
        return this;
    }

    public override ContentTypeCreateModel Build()
    {
        ContentTypeCreateModel contentType = new ContentTypeCreateModel();
        contentType.Name = GetName();
        contentType.Alias = GetAlias();
        contentType.Key = GetKey();
        contentType.ContainerKey = _containerKey;
        contentType.Cleanup = _cleanup;
        contentType.AllowedTemplateKeys = _allowedTemplateKeys ?? Array.Empty<Guid>();
        contentType.DefaultTemplateKey = _defaultTemplateKey;
        contentType.IsElement = _isElement ?? false;
        contentType.VariesByCulture = _variesByCulture ?? false;
        contentType.VariesBySegment = _variesBySegment ?? false;
        contentType.AllowedAsRoot = _allowAtRoot ?? false;
        return contentType;
    }

    public static ContentTypeCreateModel CreateBasicContentType(string alias = "umbTextpage", string name = "TextPage", IContentType parent = null)
    {
        var builder = new ContentTypeEditingBuilder();
        return (ContentTypeCreateModel)builder
            .WithAlias(alias)
            .WithName(name)
            .WithParentContentType(parent)
            .Build();
    }

    public static ContentTypeCreateModel CreateSimpleContentType(string alias = "umbTextpage", string name = "TextPage", IContentType parent = null, string propertyGroupName = "Content", Guid? defaultTemplateKey = null)
    {
        var containerKey = Guid.NewGuid();
        var builder = new ContentTypeEditingBuilder();
        return (ContentTypeCreateModel)builder
            .WithAlias(alias)
            .WithName(name)
            .WithAllowAtRoot(true)
            .WithParentContentType(parent)
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

    public static ContentTypeCreateModel CreateContentTypeWithDataTypeKey(Guid dataTypeKey, string alias = "textElement", string name = "Text Element" )
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
