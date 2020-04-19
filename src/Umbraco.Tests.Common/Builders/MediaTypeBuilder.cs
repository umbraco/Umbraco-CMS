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
                    .WithSortOrder(4)
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

        int? IWithPropertyTypeIdsIncrementingFrom.PropertyTypeIdsIncrementingFrom
        {
            get => _propertyTypeIdsIncrementingFrom;
            set => _propertyTypeIdsIncrementingFrom = value;
        }
    }
}
