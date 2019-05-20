using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Factories
{
    // factory for
    // IContentType (document types)
    // IMediaType (media types)
    // IMemberType (member types)
    //
    internal static class ContentTypeFactory
    {
        #region IContentType

        public static IContentType BuildContentTypeEntity(ContentTypeDto dto)
        {
            var contentType = new ContentType(dto.NodeDto.ParentId);

            try
            {
                contentType.DisableChangeTracking();

                BuildCommonEntity(contentType, dto);

                // reset dirty initial properties (U4-1946)
                contentType.ResetDirtyProperties(false);
                return contentType;
            }
            finally
            {
                contentType.EnableChangeTracking();
            }
        }

        #endregion

        #region IMediaType

        public static IMediaType BuildMediaTypeEntity(ContentTypeDto dto)
        {
            var contentType = new MediaType(dto.NodeDto.ParentId);
            try
            {
                contentType.DisableChangeTracking();

                BuildCommonEntity(contentType, dto);

                // reset dirty initial properties (U4-1946)
                contentType.ResetDirtyProperties(false);
            }
            finally
            {
                contentType.EnableChangeTracking();
            }

            return contentType;
        }

        #endregion

        #region IMemberType

        public static IMemberType BuildMemberTypeEntity(ContentTypeDto dto)
        {
            var contentType = new MemberType(dto.NodeDto.ParentId);
            try
            {
                contentType.DisableChangeTracking();
                BuildCommonEntity(contentType, dto, false);
                contentType.ResetDirtyProperties(false);
            }
            finally
            {
                contentType.EnableChangeTracking();
            }

            return contentType;
        }

        public static IEnumerable<MemberPropertyTypeDto> BuildMemberPropertyTypeDtos(IMemberType entity)
        {
            var memberType = entity as MemberType;
            if (memberType == null || memberType.PropertyTypes.Any() == false)
                return Enumerable.Empty<MemberPropertyTypeDto>();

            var dtos = memberType.PropertyTypes.Select(x => new MemberPropertyTypeDto
            {
                NodeId = entity.Id,
                PropertyTypeId = x.Id,
                CanEdit = memberType.MemberCanEditProperty(x.Alias),
                ViewOnProfile = memberType.MemberCanViewProperty(x.Alias),
                IsSensitive = memberType.IsSensitiveProperty(x.Alias)
            }).ToList();
            return dtos;
        }

        #endregion

        #region Common

        private static void BuildCommonEntity(ContentTypeBase entity, ContentTypeDto dto, bool setVariations = true)
        {
            entity.Id = dto.NodeDto.NodeId;
            entity.Key = dto.NodeDto.UniqueId;
            entity.Alias = dto.Alias;
            entity.Name = dto.NodeDto.Text;
            entity.Icon = dto.Icon;
            entity.SortOrder = dto.NodeDto.SortOrder;
            entity.Description = dto.Description;
            entity.CreateDate = dto.NodeDto.CreateDate;
            entity.UpdateDate = dto.NodeDto.CreateDate;
            entity.Path = dto.NodeDto.Path;
            entity.Level = dto.NodeDto.Level;
            entity.CreatorId = dto.NodeDto.UserId ?? Constants.Security.UnknownUserId;
            entity.AllowedAsRoot = dto.AllowAtRoot;
            entity.IsContainer = dto.IsContainer;
            entity.IsElement = dto.IsElement;
            entity.Trashed = dto.NodeDto.Trashed;

            if (setVariations)
                entity.Variations = (ContentVariation) dto.Variations;
        }

        public static ContentTypeDto BuildContentTypeDto(IContentTypeBase entity)
        {
            Guid nodeObjectType;
            if (entity is IContentType)
                nodeObjectType = Constants.ObjectTypes.DocumentType;
            else if (entity is IMediaType)
                nodeObjectType = Constants.ObjectTypes.MediaType;
            else if (entity is IMemberType)
                nodeObjectType = Constants.ObjectTypes.MemberType;
            else
                throw new Exception("Invalid entity.");

            var contentTypeDto = new ContentTypeDto
            {
                Alias = entity.Alias,
                Description = entity.Description,
                Icon = entity.Icon,
                Thumbnail = entity.Thumbnail,
                NodeId = entity.Id,
                AllowAtRoot = entity.AllowedAsRoot,
                IsContainer = entity.IsContainer,
                IsElement = entity.IsElement,
                Variations = (byte) entity.Variations,
                NodeDto = BuildNodeDto(entity, nodeObjectType)
            };
            return contentTypeDto;
        }

        private static NodeDto BuildNodeDto(IUmbracoEntity entity, Guid nodeObjectType)
        {
            var nodeDto = new NodeDto
            {
                CreateDate = entity.CreateDate,
                NodeId = entity.Id,
                Level = short.Parse(entity.Level.ToString(CultureInfo.InvariantCulture)),
                NodeObjectType = nodeObjectType,
                ParentId = entity.ParentId,
                Path = entity.Path,
                SortOrder = entity.SortOrder,
                Text = entity.Name,
                Trashed = false,
                UniqueId = entity.Key,
                UserId = entity.CreatorId
            };
            return nodeDto;
        }

        #endregion
    }
}
