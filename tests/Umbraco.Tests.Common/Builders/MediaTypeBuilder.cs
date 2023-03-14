// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class MediaTypeBuilder
    : ContentTypeBaseBuilder<MediaBuilder, IMediaType>,
        IWithPropertyTypeIdsIncrementingFrom
{
    private readonly List<PropertyGroupBuilder<MediaTypeBuilder>> _propertyGroupBuilders = new();
    private int? _propertyTypeIdsIncrementingFrom;

    public MediaTypeBuilder()
        : base(null)
    {
    }

    public MediaTypeBuilder(MediaBuilder parentBuilder)
        : base(parentBuilder)
    {
    }

    int? IWithPropertyTypeIdsIncrementingFrom.PropertyTypeIdsIncrementingFrom
    {
        get => _propertyTypeIdsIncrementingFrom;
        set => _propertyTypeIdsIncrementingFrom = value;
    }

    public MediaTypeBuilder WithMediaPropertyGroup()
    {
        var builder = new PropertyGroupBuilder<MediaTypeBuilder>(this)
            .WithId(99)
            .WithName("Media")
            .WithSortOrder(1)
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.UploadField)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithAlias(Constants.Conventions.Media.File)
            .WithName("File")
            .WithSortOrder(1)
            .WithDataTypeId(-90)
            .Done()
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .WithValueStorageType(ValueStorageType.Integer)
            .WithAlias(Constants.Conventions.Media.Width)
            .WithName("Width")
            .WithSortOrder(2)
            .WithDataTypeId(Constants.System.DefaultLabelDataTypeId)
            .Done()
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .WithValueStorageType(ValueStorageType.Integer)
            .WithAlias(Constants.Conventions.Media.Height)
            .WithName("Height")
            .WithSortOrder(3)
            .WithDataTypeId(Constants.System.DefaultLabelDataTypeId)
            .Done()
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .WithValueStorageType(ValueStorageType.Integer)
            .WithAlias(Constants.Conventions.Media.Bytes)
            .WithName("Bytes")
            .WithSortOrder(4)
            .WithDataTypeId(Constants.System.DefaultLabelDataTypeId)
            .Done()
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithAlias(Constants.Conventions.Media.Extension)
            .WithName("File Extension")
            .WithSortOrder(5)
            .WithDataTypeId(Constants.System.DefaultLabelDataTypeId)
            .Done();
        _propertyGroupBuilders.Add(builder);
        return this;
    }

    public PropertyGroupBuilder<MediaTypeBuilder> AddPropertyGroup()
    {
        var builder = new PropertyGroupBuilder<MediaTypeBuilder>(this);
        _propertyGroupBuilders.Add(builder);
        return builder;
    }

    public override IMediaType Build()
    {
        MediaType mediaType;
        var parent = GetParent();
        if (parent != null)
        {
            mediaType = new MediaType(ShortStringHelper, (IMediaType)parent, GetAlias());
        }
        else
        {
            mediaType = new MediaType(ShortStringHelper, GetParentId()) { Alias = GetAlias() };
        }

        mediaType.Id = GetId();
        mediaType.Key = GetKey();
        mediaType.CreateDate = GetCreateDate();
        mediaType.UpdateDate = GetUpdateDate();
        mediaType.Alias = GetAlias();
        mediaType.Name = GetName();
        mediaType.Level = GetLevel();
        mediaType.Path = GetPath();
        mediaType.SortOrder = GetSortOrder();
        mediaType.Description = GetDescription();
        mediaType.Icon = GetIcon();
        mediaType.Thumbnail = GetThumbnail();
        mediaType.CreatorId = GetCreatorId();
        mediaType.Trashed = GetTrashed();
        mediaType.IsContainer = GetIsContainer();

        BuildPropertyGroups(mediaType, _propertyGroupBuilders.Select(x => x.Build()));
        BuildPropertyTypeIds(mediaType, _propertyTypeIdsIncrementingFrom);

        mediaType.ResetDirtyProperties(false);

        return mediaType;
    }

    public static MediaType CreateSimpleMediaType(
        string alias,
        string name,
        IMediaType parent = null,
        bool randomizeAliases = false,
        string propertyGroupAlias = "content",
        string propertyGroupName = "Content")
    {
        var builder = new MediaTypeBuilder();
        var mediaType = builder
            .WithAlias(alias)
            .WithName(name)
            .WithParentContentType(parent)
            .AddPropertyGroup()
            .WithAlias(propertyGroupAlias)
            .WithName(propertyGroupName)
            .WithSortOrder(1)
            .AddPropertyType()
            .WithAlias(RandomAlias("title", randomizeAliases))
            .WithName("Title")
            .WithSortOrder(1)
            .Done()
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TinyMce)
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithAlias(RandomAlias("bodyText", randomizeAliases))
            .WithName("Body text")
            .WithSortOrder(2)
            .WithDataTypeId(-87)
            .Done()
            .AddPropertyType()
            .WithAlias(RandomAlias("author", randomizeAliases))
            .WithName("Author")
            .WithSortOrder(3)
            .Done()
            .Done()
            .Build();

        // Ensure that nothing is marked as dirty
        mediaType.ResetDirtyProperties(false);

        return (MediaType)mediaType;
    }

    public static MediaType CreateImageMediaType(string alias = Constants.Conventions.MediaTypes.Image) =>
        CreateImageMediaType(alias ?? "Image", Constants.PropertyEditors.Aliases.UploadField, -90);

    public static MediaType CreateImageMediaTypeWithCrop(string alias = Constants.Conventions.MediaTypes.Image) =>
        CreateImageMediaType(alias ?? "Image", Constants.PropertyEditors.Aliases.ImageCropper, 1043);

    private static MediaType CreateImageMediaType(string alias, string imageFieldPropertyEditorAlias, int imageFieldDataTypeId)
    {
        var builder = new MediaTypeBuilder();
        var mediaType = builder
            .WithAlias(alias)
            .WithName("Image")
            .AddPropertyGroup()
            .WithName("Media")
            .WithSortOrder(1)
            .AddPropertyType()
            .WithPropertyEditorAlias(imageFieldPropertyEditorAlias)
            .WithAlias(Constants.Conventions.Media.File)
            .WithName("File")
            .WithSortOrder(1)
            .WithDataTypeId(imageFieldDataTypeId)
            .Done()
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .WithAlias(Constants.Conventions.Media.Width)
            .WithName("Width")
            .WithSortOrder(2)
            .WithDataTypeId(Constants.System.DefaultLabelDataTypeId)
            .Done()
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .WithAlias(Constants.Conventions.Media.Height)
            .WithName("Height")
            .WithSortOrder(3)
            .WithDataTypeId(Constants.System.DefaultLabelDataTypeId)
            .Done()
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .WithAlias(Constants.Conventions.Media.Bytes)
            .WithName("Bytes")
            .WithSortOrder(4)
            .WithDataTypeId(Constants.System.DefaultLabelDataTypeId)
            .Done()
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .WithAlias(Constants.Conventions.Media.Extension)
            .WithName("File Extension")
            .WithSortOrder(5)
            .WithDataTypeId(Constants.System.DefaultLabelDataTypeId)
            .Done()
            .Done()
            .Build();

        // Ensure that nothing is marked as dirty
        mediaType.ResetDirtyProperties(false);

        return (MediaType)mediaType;
    }

    public static MediaType CreateVideoMediaType()
    {
        var builder = new MediaTypeBuilder();
        var mediaType = builder
            .WithAlias("video")
            .WithName("Video")
            .AddPropertyGroup()
            .WithName("Media")
            .WithSortOrder(1)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithSortOrder(1)
            .Done()
            .AddPropertyType()
            .WithAlias("videoFile")
            .WithName("Video file")
            .WithSortOrder(1)
            .Done()
            .Done()
            .Build();

        // Ensure that nothing is marked as dirty
        mediaType.ResetDirtyProperties(false);

        return (MediaType)mediaType;
    }

    public static MediaType CreateNewMediaType()
    {
        var builder = new MediaTypeBuilder();
        var mediaType = builder
            .WithAlias("newMediaType")
            .WithName("New Media Type")
            .AddPropertyGroup()
            .WithAlias("media")
            .WithName("Media")
            .WithSortOrder(1)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithSortOrder(1)
            .Done()
            .AddPropertyType()
            .WithAlias("videoFile")
            .WithName("Video file")
            .WithSortOrder(1)
            .Done()
            .Done()
            .Build();

        // Ensure that nothing is marked as dirty
        mediaType.ResetDirtyProperties(false);

        return (MediaType)mediaType;
    }
}
