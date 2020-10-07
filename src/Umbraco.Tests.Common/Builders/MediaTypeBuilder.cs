using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class MediaTypeBuilder
        : ContentTypeBaseBuilder<MediaBuilder, IMediaType>,
            IWithPropertyTypeIdsIncrementingFrom
    {
        private List<PropertyGroupBuilder<MediaTypeBuilder>> _propertyGroupBuilders = new List<PropertyGroupBuilder<MediaTypeBuilder>>();
        private int? _propertyTypeIdsIncrementingFrom;

        public MediaTypeBuilder() : base(null)
        {
        }

        public MediaTypeBuilder(MediaBuilder parentBuilder) : base(parentBuilder)
        {
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
            var mediaType = new MediaType(ShortStringHelper, GetParentId())
            {
                Id = GetId(),
                Key = GetKey(),
                CreateDate = GetCreateDate(),
                UpdateDate = GetUpdateDate(),
                Alias = GetAlias(),
                Name = GetName(),
                Level = GetLevel(),
                Path = GetPath(),
                SortOrder = GetSortOrder(),
                Description = GetDescription(),
                Icon = GetIcon(),
                Thumbnail = GetThumbnail(),
                CreatorId = GetCreatorId(),
                Trashed = GetTrashed(),
                IsContainer = GetIsContainer(),
            };

            BuildPropertyGroups(mediaType, _propertyGroupBuilders.Select(x => x.Build()));
            BuildPropertyTypeIds(mediaType, _propertyTypeIdsIncrementingFrom);

            mediaType.ResetDirtyProperties(false);

            return mediaType;
        }

        public static MediaType CreateSimpleMediaType(string alias, string name, IMediaType parent = null, bool randomizeAliases = false, string propertyGroupName = "Content")
        {
            var builder = new MediaTypeBuilder();
            var mediaType = builder
                .WithAlias(alias)
                .WithName(name)
                .WithParentId(parent?.Id ?? -1)
                .AddPropertyGroup()
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

        public static MediaType CreateImageMediaType(string alias = Constants.Conventions.MediaTypes.Image)
        {
            return CreateImageMediaType(alias ?? "Image", Constants.PropertyEditors.Aliases.UploadField, -90);
        }

        public static MediaType CreateImageMediaTypeWithCrop(string alias = Constants.Conventions.MediaTypes.Image)
        {
            return CreateImageMediaType(alias ?? "Image", Constants.PropertyEditors.Aliases.ImageCropper, 1043);
        }

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

        int? IWithPropertyTypeIdsIncrementingFrom.PropertyTypeIdsIncrementingFrom
        {
            get => _propertyTypeIdsIncrementingFrom;
            set => _propertyTypeIdsIncrementingFrom = value;
        }
    }
}
