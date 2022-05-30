// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentTypeBuilder
    : ContentTypeBaseBuilder<ContentBuilder, IContentType>,
        IWithPropertyTypeIdsIncrementingFrom,
        IBuildPropertyTypes
{
    private readonly List<ContentTypeSortBuilder> _allowedContentTypeBuilders = new();
    private readonly List<PropertyTypeBuilder<ContentTypeBuilder>> _noGroupPropertyTypeBuilders = new();
    private readonly List<PropertyGroupBuilder<ContentTypeBuilder>> _propertyGroupBuilders = new();
    private readonly List<TemplateBuilder> _templateBuilders = new();
    private ContentVariation? _contentVariation;
    private int? _defaultTemplateId;
    private PropertyTypeCollection _propertyTypeCollection;

    private int? _propertyTypeIdsIncrementingFrom;

    public ContentTypeBuilder()
        : base(null)
    {
    }

    public ContentTypeBuilder(ContentBuilder parentBuilder)
        : base(parentBuilder)
    {
    }

    int? IWithPropertyTypeIdsIncrementingFrom.PropertyTypeIdsIncrementingFrom
    {
        get => _propertyTypeIdsIncrementingFrom;
        set => _propertyTypeIdsIncrementingFrom = value;
    }

    public ContentTypeBuilder WithDefaultTemplateId(int templateId)
    {
        _defaultTemplateId = templateId;
        return this;
    }

    public ContentTypeBuilder WithContentVariation(ContentVariation contentVariation)
    {
        _contentVariation = contentVariation;
        return this;
    }

    public ContentTypeBuilder WithPropertyTypeCollection(PropertyTypeCollection propertyTypeCollection)
    {
        _propertyTypeCollection = propertyTypeCollection;
        return this;
    }

    public PropertyGroupBuilder<ContentTypeBuilder> AddPropertyGroup()
    {
        var builder = new PropertyGroupBuilder<ContentTypeBuilder>(this);
        _propertyGroupBuilders.Add(builder);
        return builder;
    }

    public PropertyTypeBuilder<ContentTypeBuilder> AddPropertyType()
    {
        var builder = new PropertyTypeBuilder<ContentTypeBuilder>(this);
        _noGroupPropertyTypeBuilders.Add(builder);
        return builder;
    }

    public TemplateBuilder AddAllowedTemplate()
    {
        var builder = new TemplateBuilder(this);
        _templateBuilders.Add(builder);
        return builder;
    }

    public ContentTypeSortBuilder AddAllowedContentType()
    {
        var builder = new ContentTypeSortBuilder(this);
        _allowedContentTypeBuilders.Add(builder);
        return builder;
    }

    public override IContentType Build()
    {
        var contentVariation = _contentVariation ?? ContentVariation.Nothing;

        ContentType contentType;
        var parent = GetParent();
        if (parent != null)
        {
            contentType = new ContentType(ShortStringHelper, (IContentType)parent, GetAlias());
        }
        else
        {
            contentType = new ContentType(ShortStringHelper, GetParentId()) { Alias = GetAlias() };
        }

        contentType.Id = GetId();
        contentType.Key = GetKey();
        contentType.CreateDate = GetCreateDate();
        contentType.UpdateDate = GetUpdateDate();
        contentType.Name = GetName();
        contentType.Level = GetLevel();
        contentType.Path = GetPath();
        contentType.SortOrder = GetSortOrder();
        contentType.Description = GetDescription();
        contentType.Icon = GetIcon();
        contentType.Thumbnail = GetThumbnail();
        contentType.CreatorId = GetCreatorId();
        contentType.Trashed = GetTrashed();
        contentType.IsContainer = GetIsContainer();
        contentType.HistoryCleanup = new HistoryCleanup();

        contentType.Variations = contentVariation;

        if (_propertyTypeCollection != null)
        {
            var propertyGroup = new PropertyGroupBuilder()
                .WithAlias("content")
                .WithName("Content")
                .WithSortOrder(1)
                .WithPropertyTypeCollection(_propertyTypeCollection)
                .Build();
            contentType.PropertyGroups.Add(propertyGroup);
        }
        else
        {
            contentType.NoGroupPropertyTypes = _noGroupPropertyTypeBuilders.Select(x => x.Build());
            BuildPropertyGroups(contentType, _propertyGroupBuilders.Select(x => x.Build()));
            BuildPropertyTypeIds(contentType, _propertyTypeIdsIncrementingFrom);
        }

        contentType.AllowedContentTypes = _allowedContentTypeBuilders.Select(x => x.Build());

        contentType.AllowedTemplates = _templateBuilders.Select(x => x.Build());
        if (_defaultTemplateId.HasValue)
        {
            contentType.SetDefaultTemplate(contentType.AllowedTemplates
                .SingleOrDefault(x => x.Id == _defaultTemplateId.Value));
        }

        contentType.ResetDirtyProperties(false);

        return contentType;
    }

    public static ContentType CreateBasicContentType(string alias = "basePage", string name = "Base Page", IContentType parent = null)
    {
        var builder = new ContentTypeBuilder();
        return (ContentType)builder
            .WithAlias(alias)
            .WithName(name)
            .WithParentContentType(parent)
            .Build();
    }

    public static ContentType CreateSimpleContentType2(string alias, string name, IContentType parent = null, bool randomizeAliases = false, string propertyGroupAlias = "content", string propertyGroupName = "Content")
    {
        var builder = CreateSimpleContentTypeHelper(alias, name, parent, randomizeAliases: randomizeAliases, propertyGroupAlias: propertyGroupAlias, propertyGroupName: propertyGroupName);

        builder.AddPropertyType()
            .WithAlias(RandomAlias("gen", randomizeAliases))
            .WithName("Gen")
            .WithSortOrder(1)
            .WithDataTypeId(-88)
            .WithMandatory(false)
            .WithDescription(string.Empty)
            .WithLabelOnTop(true)
            .Done();

        return (ContentType)builder.Build();
    }

    public static ContentType CreateSimpleContentType(
        string alias = null,
        string name = null,
        IContentType parent = null,
        PropertyTypeCollection propertyTypeCollection = null,
        bool randomizeAliases = false,
        string propertyGroupAlias = "content",
        string propertyGroupName = "Content",
        bool mandatoryProperties = false,
        int defaultTemplateId = 0) =>
        (ContentType)CreateSimpleContentTypeHelper(alias, name, parent, propertyTypeCollection, randomizeAliases, propertyGroupAlias, propertyGroupName, mandatoryProperties, defaultTemplateId).Build();

    public static ContentTypeBuilder CreateSimpleContentTypeHelper(
        string alias = null,
        string name = null,
        IContentType parent = null,
        PropertyTypeCollection propertyTypeCollection = null,
        bool randomizeAliases = false,
        string propertyGroupAlias = "content",
        string propertyGroupName = "Content",
        bool mandatoryProperties = false,
        int defaultTemplateId = 0)
    {
        var builder = new ContentTypeBuilder()
            .WithAlias(alias ?? "simple")
            .WithName(name ?? "Simple Page")
            .WithParentContentType(parent);

        if (propertyTypeCollection != null)
        {
            builder = builder
                .WithPropertyTypeCollection(propertyTypeCollection);
        }
        else
        {
            builder = builder
                .AddPropertyGroup()
                .WithAlias(propertyGroupAlias)
                .WithName(propertyGroupName)
                .WithSortOrder(1)
                .WithSupportsPublishing(true)
                .AddPropertyType()
                .WithAlias(RandomAlias("title", randomizeAliases))
                .WithName("Title")
                .WithSortOrder(1)
                .WithMandatory(mandatoryProperties)
                .WithLabelOnTop(true)
                .Done()
                .AddPropertyType()
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TinyMce)
                .WithValueStorageType(ValueStorageType.Ntext)
                .WithAlias(RandomAlias("bodyText", randomizeAliases))
                .WithName("Body text")
                .WithSortOrder(2)
                .WithDataTypeId(Constants.DataTypes.RichtextEditor)
                .WithMandatory(mandatoryProperties)
                .WithLabelOnTop(true)
                .Done()
                .AddPropertyType()
                .WithAlias(RandomAlias("author", randomizeAliases))
                .WithName("Author")
                .WithSortOrder(3)
                .WithMandatory(mandatoryProperties)
                .WithLabelOnTop(true)
                .Done()
                .Done();
        }

        builder = builder
            .AddAllowedTemplate()
            .WithId(defaultTemplateId)
            .WithAlias("textPage")
            .WithName("Textpage")
            .Done()
            .WithDefaultTemplateId(defaultTemplateId);

        return builder;
    }

    public static ContentType CreateSimpleTagsContentType(string alias, string name, IContentType parent = null, bool randomizeAliases = false, string propertyGroupName = "Content", int defaultTemplateId = 1)
    {
        var contentType = CreateSimpleContentType(alias, name, parent, randomizeAliases: randomizeAliases, propertyGroupName: propertyGroupName, defaultTemplateId: defaultTemplateId);

        var propertyType = new PropertyTypeBuilder()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Tags)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithAlias(RandomAlias("tags", randomizeAliases))
            .WithName("Tags")
            .WithDataTypeId(Constants.DataTypes.Tags)
            .WithSortOrder(99)
            .Build();
        contentType.AddPropertyType(propertyType);

        return contentType;
    }

    public static ContentType CreateTextPageContentType(string alias = "textPage", string name = "Text Page", int defaultTemplateId = 1)
    {
        var builder = new ContentTypeBuilder();
        return (ContentType)builder
            .WithAlias(alias)
            .WithName(name)
            .AddPropertyGroup()
            .WithAlias("content")
            .WithName("Content")
            .WithSortOrder(1)
            .WithSupportsPublishing(true)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithSortOrder(1)
            .Done()
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TinyMce)
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithAlias("bodyText")
            .WithName("Body text")
            .WithSortOrder(2)
            .WithDataTypeId(Constants.DataTypes.RichtextEditor)
            .Done()
            .Done()
            .AddPropertyGroup()
            .WithName("Meta")
            .WithAlias("meta")
            .WithSortOrder(2)
            .WithSupportsPublishing(true)
            .AddPropertyType()
            .WithAlias("keywords")
            .WithName("Keywords")
            .WithSortOrder(1)
            .Done()
            .AddPropertyType()
            .WithAlias("description")
            .WithName("Description")
            .WithSortOrder(2)
            .Done()
            .Done()
            .AddAllowedTemplate()
            .WithId(defaultTemplateId)
            .WithAlias("textpage")
            .WithName("Textpage")
            .Done()
            .WithDefaultTemplateId(defaultTemplateId)
            .Build();
    }

    public static ContentType CreateMetaContentType(string alias = "meta", string name = "Meta")
    {
        var builder = new ContentTypeBuilder();
        return (ContentType)builder
            .WithAlias(alias)
            .WithName(name)
            .WithDescription($"ContentType used for {name} tags")
            .AddPropertyGroup()
            .WithAlias(alias)
            .WithName(name)
            .WithSortOrder(2)
            .WithSupportsPublishing(true)
            .AddPropertyType()
            .WithAlias($"{alias}keywords")
            .WithName($"{name} Keywords")
            .WithSortOrder(1)
            .Done()
            .AddPropertyType()
            .WithAlias($"{alias}description")
            .WithName($"{name} Description")
            .WithSortOrder(2)
            .Done()
            .Done()
            .Build();
    }

    public static ContentType CreateContentMetaContentType()
    {
        var builder = new ContentTypeBuilder();
        return (ContentType)builder
            .WithAlias("contentMeta")
            .WithName("Content Meta")
            .WithDescription("ContentType used for Content Meta")
            .AddPropertyGroup()
            .WithName("Content")
            .WithSortOrder(2)
            .WithSupportsPublishing(true)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithSortOrder(1)
            .Done()
            .Done()
            .Build();
    }

    public static ContentType CreateAllTypesContentType(string alias, string name)
    {
        var builder = new ContentTypeBuilder();
        return (ContentType)builder
            .WithAlias(alias)
            .WithName(name)
            .AddPropertyGroup()
            .WithName("Content")
            .WithSupportsPublishing(true)
            .AddPropertyType()
            .WithAlias("isTrue")
            .WithName("Is True or False")
            .WithDataTypeId(Constants.DataTypes.Boolean)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Boolean)
            .WithValueStorageType(ValueStorageType.Integer)
            .WithSortOrder(1)
            .Done()
            .AddPropertyType()
            .WithAlias("number")
            .WithName("Number")
            .WithDataTypeId(-51)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Integer)
            .WithValueStorageType(ValueStorageType.Integer)
            .WithSortOrder(2)
            .Done()
            .AddPropertyType()
            .WithAlias("bodyText")
            .WithName("Body Text")
            .WithDataTypeId(Constants.DataTypes.RichtextEditor)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TinyMce)
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithSortOrder(3)
            .Done()
            .AddPropertyType()
            .WithAlias("singleLineText")
            .WithName("Text String")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithSortOrder(4)
            .Done()
            .AddPropertyType()
            .WithAlias("multilineText")
            .WithName("Multiple Text Strings")
            .WithDataTypeId(Constants.DataTypes.Textarea)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextArea)
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithSortOrder(5)
            .Done()
            .AddPropertyType()
            .WithAlias("upload")
            .WithName("Upload Field")
            .WithDataTypeId(Constants.DataTypes.Upload)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.UploadField)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithSortOrder(6)
            .Done()
            .AddPropertyType()
            .WithAlias("label")
            .WithName("Label")
            .WithDataTypeId(Constants.DataTypes.LabelString)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithSortOrder(7)
            .Done()
            .AddPropertyType()
            .WithAlias("dateTime")
            .WithName("Date Time")
            .WithDataTypeId(Constants.DataTypes.DateTime)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DateTime)
            .WithValueStorageType(ValueStorageType.Date)
            .WithSortOrder(8)
            .Done()
            .AddPropertyType()
            .WithAlias("colorPicker")
            .WithName("Color Picker")
            .WithDataTypeId(-37)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ColorPicker)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithSortOrder(9)
            .Done()
            .AddPropertyType()
            .WithAlias("ddlMultiple")
            .WithName("Dropdown List Multiple")
            .WithDataTypeId(Constants.DataTypes.DropDownMultiple)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DropDownListFlexible)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithSortOrder(11)
            .Done()
            .AddPropertyType()
            .WithAlias("rbList")
            .WithName("Radio Button List")
            .WithDataTypeId(-40)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.RadioButtonList)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithSortOrder(12)
            .Done()
            .AddPropertyType()
            .WithAlias("date")
            .WithName("Date")
            .WithDataTypeId(-36)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DateTime)
            .WithValueStorageType(ValueStorageType.Date)
            .WithSortOrder(13)
            .Done()
            .AddPropertyType()
            .WithAlias("ddl")
            .WithName("Dropdown List")
            .WithDataTypeId(Constants.DataTypes.DropDownSingle)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DropDownListFlexible)
            .WithValueStorageType(ValueStorageType.Integer)
            .WithSortOrder(14)
            .Done()
            .AddPropertyType()
            .WithAlias("chklist")
            .WithName("Checkbox List")
            .WithDataTypeId(-43)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.CheckBoxList)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithSortOrder(15)
            .Done()
            .AddPropertyType()
            .WithAlias("contentPicker")
            .WithName("Content Picker")
            .WithDataTypeId(1046)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ContentPicker)
            .WithValueStorageType(ValueStorageType.Integer)
            .WithSortOrder(16)
            .Done()
            .AddPropertyType()
            .WithAlias("mediaPicker")
            .WithName("Media Picker")
            .WithDataTypeId(1048)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.MediaPicker)
            .WithValueStorageType(ValueStorageType.Integer)
            .WithSortOrder(17)
            .Done()
            .AddPropertyType()
            .WithAlias("memberPicker")
            .WithName("Member Picker")
            .WithDataTypeId(1047)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.MemberPicker)
            .WithValueStorageType(ValueStorageType.Integer)
            .WithSortOrder(18)
            .Done()
            .AddPropertyType()
            .WithAlias("multiUrlPicker")
            .WithName("Multi URL Picker")
            .WithDataTypeId(1050)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.MultiUrlPicker)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithSortOrder(19)
            .Done()
            .AddPropertyType()
            .WithAlias("tags")
            .WithName("Tags")
            .WithDataTypeId(Constants.DataTypes.Tags)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Tags)
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithSortOrder(20)
            .Done()
            .Done()
            .Build();
    }
}
