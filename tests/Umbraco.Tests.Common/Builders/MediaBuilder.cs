// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;
using Umbraco.Cms.Tests.Common.Extensions;

namespace Umbraco.Cms.Tests.Common.Builders;

public class MediaBuilder
    : BuilderBase<Media>,
        IBuildContentTypes,
        IWithIdBuilder,
        IWithKeyBuilder,
        IWithParentIdBuilder,
        IWithCreatorIdBuilder,
        IWithCreateDateBuilder,
        IWithUpdateDateBuilder,
        IWithNameBuilder,
        IWithTrashedBuilder,
        IWithLevelBuilder,
        IWithPathBuilder,
        IWithSortOrderBuilder,
        IWithPropertyValues
{
    private DateTime? _createDate;
    private int? _creatorId;

    private int? _id;
    private Guid? _key;
    private int? _level;
    private IMediaType _mediaType;
    private MediaTypeBuilder _mediaTypeBuilder;
    private string _name;
    private int? _parentId;
    private string _path;
    private GenericDictionaryBuilder<MediaBuilder, string, object> _propertyDataBuilder;
    private object _propertyValues;
    private string _propertyValuesCulture;
    private string _propertyValuesSegment;
    private int? _sortOrder;
    private bool? _trashed;
    private DateTime? _updateDate;

    DateTime? IWithCreateDateBuilder.CreateDate
    {
        get => _createDate;
        set => _createDate = value;
    }

    int? IWithCreatorIdBuilder.CreatorId
    {
        get => _creatorId;
        set => _creatorId = value;
    }

    int? IWithIdBuilder.Id
    {
        get => _id;
        set => _id = value;
    }

    Guid? IWithKeyBuilder.Key
    {
        get => _key;
        set => _key = value;
    }

    int? IWithLevelBuilder.Level
    {
        get => _level;
        set => _level = value;
    }

    string IWithNameBuilder.Name
    {
        get => _name;
        set => _name = value;
    }

    int? IWithParentIdBuilder.ParentId
    {
        get => _parentId;
        set => _parentId = value;
    }

    string IWithPathBuilder.Path
    {
        get => _path;
        set => _path = value;
    }

    object IWithPropertyValues.PropertyValues
    {
        get => _propertyValues;
        set => _propertyValues = value;
    }

    string IWithPropertyValues.PropertyValuesCulture
    {
        get => _propertyValuesCulture;
        set => _propertyValuesCulture = value;
    }

    string IWithPropertyValues.PropertyValuesSegment
    {
        get => _propertyValuesSegment;
        set => _propertyValuesSegment = value;
    }

    int? IWithSortOrderBuilder.SortOrder
    {
        get => _sortOrder;
        set => _sortOrder = value;
    }

    bool? IWithTrashedBuilder.Trashed
    {
        get => _trashed;
        set => _trashed = value;
    }

    DateTime? IWithUpdateDateBuilder.UpdateDate
    {
        get => _updateDate;
        set => _updateDate = value;
    }

    public MediaTypeBuilder AddMediaType()
    {
        _mediaType = null;
        var builder = new MediaTypeBuilder(this);
        _mediaTypeBuilder = builder;
        return builder;
    }

    public MediaBuilder WithMediaType(IMediaType mediaType)
    {
        _mediaTypeBuilder = null;
        _mediaType = mediaType;

        return this;
    }

    public GenericDictionaryBuilder<MediaBuilder, string, object> AddPropertyData()
    {
        var builder = new GenericDictionaryBuilder<MediaBuilder, string, object>(this);
        _propertyDataBuilder = builder;
        return builder;
    }

    public override Media Build()
    {
        var id = _id ?? 0;
        var key = _key ?? Guid.NewGuid();
        var parentId = _parentId ?? -1;
        var createDate = _createDate ?? DateTime.Now;
        var updateDate = _updateDate ?? DateTime.Now;
        var name = _name ?? Guid.NewGuid().ToString();
        var creatorId = _creatorId ?? 0;
        var level = _level ?? 1;
        var path = _path ?? $"-1,{id}";
        var sortOrder = _sortOrder ?? 0;
        var trashed = _trashed ?? false;
        var propertyValues = _propertyValues;
        var propertyValuesCulture = _propertyValuesCulture;
        var propertyValuesSegment = _propertyValuesSegment;

        if (_mediaTypeBuilder is null && _mediaType is null)
        {
            throw new InvalidOperationException(
                "A media item cannot be constructed without providing a media type. Use AddMediaType() or WithMediaType().");
        }

        var mediaType = _mediaType ?? _mediaTypeBuilder.Build();

        var media = new Media(name, parentId, mediaType)
        {
            Id = id,
            Key = key,
            CreateDate = createDate,
            UpdateDate = updateDate,
            CreatorId = creatorId,
            Level = level,
            Path = path,
            SortOrder = sortOrder,
            Trashed = trashed
        };

        if (_propertyDataBuilder != null || propertyValues != null)
        {
            if (_propertyDataBuilder != null)
            {
                var propertyData = _propertyDataBuilder.Build();
                foreach (var keyValuePair in propertyData)
                {
                    media.SetValue(keyValuePair.Key, keyValuePair.Value);
                }
            }
            else
            {
                media.PropertyValues(propertyValues, propertyValuesCulture, propertyValuesSegment);
            }

            media.ResetDirtyProperties(false);
        }

        return media;
    }

    public static Media CreateSimpleMedia(IMediaType mediaType, string name, int parentId, int id = 0) =>
        new MediaBuilder()
            .WithId(id)
            .WithName(name)
            .WithMediaType(mediaType)
            .WithParentId(parentId)
            .WithPropertyValues(new { title = name + " Subpage", bodyText = "This is a subpage", author = "John Doe" })
            .Build();

    public static Media CreateMediaImage(IMediaType mediaType, int parentId) =>
        CreateMediaImage(mediaType, parentId, "/media/test-image.png");

    public static Media CreateMediaImageWithCrop(IMediaType mediaType, int parentId) =>
        CreateMediaImage(mediaType, parentId, "{src: '/media/test-image.png', crops: []}");

    private static Media CreateMediaImage(IMediaType mediaType, int parentId, string fileValue) => new MediaBuilder()
        .WithMediaType(mediaType)
        .WithName("Test Image")
        .WithParentId(parentId)
        .AddPropertyData()
        .WithKeyValue(Constants.Conventions.Media.File, fileValue)
        .WithKeyValue(Constants.Conventions.Media.Width, "200")
        .WithKeyValue(Constants.Conventions.Media.Height, "200")
        .WithKeyValue(Constants.Conventions.Media.Bytes, "100")
        .WithKeyValue(Constants.Conventions.Media.Extension, "png")
        .Done()
        .Build();

    public static Media CreateMediaFolder(IMediaType mediaType, int parentId) => new MediaBuilder()
        .WithMediaType(mediaType)
        .WithName("Test Folder")
        .WithParentId(parentId)
        .Build();

    public static Media CreateMediaFile(IMediaType mediaType, int parentId) => new MediaBuilder()
        .WithMediaType(mediaType)
        .WithName("Test File")
        .WithParentId(parentId)
        .AddPropertyData()
        .WithKeyValue(Constants.Conventions.Media.File, "/media/test-file.txt")
        .WithKeyValue(Constants.Conventions.Media.Bytes, "100")
        .WithKeyValue(Constants.Conventions.Media.Extension, "png")
        .Done()
        .Build();
}
