using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MediaFactory
    {
        /// <summary>
        /// Builds an IMedia item from a dto and content type.
        /// </summary>
        public static Models.Media BuildEntity(ContentDto dto, IMediaType contentType)
        {
            var nodeDto = dto.NodeDto;
            var contentVersionDto = dto.ContentVersionDto;

            var content = new Models.Media(nodeDto.Text, nodeDto.ParentId, contentType);

            try
            {
                content.DisableChangeTracking();

                content.Id = dto.NodeId;
                content.Key = nodeDto.UniqueId;
                content.Version = contentVersionDto.VersionId;

                // fixme missing names?

                content.Path = nodeDto.Path;
                content.Level = nodeDto.Level;
                content.ParentId = nodeDto.ParentId;
                content.SortOrder = nodeDto.SortOrder;
                content.Trashed = nodeDto.Trashed;

                content.CreatorId = nodeDto.UserId ?? 0;
                // fixme missing writerId - which then should move to nodeDto
                content.CreateDate = nodeDto.CreateDate;
                content.UpdateDate = contentVersionDto.VersionDate;

                // reset dirty initial properties (U4-1946)
                content.ResetDirtyProperties(false);
                return content;
            }
            finally
            {
                content.EnableChangeTracking();
            }
        }

        /// <summary>
        /// Buils a dto from an IMedia item.
        /// </summary>
        public static ContentDto BuildDto(IMedia entity)
        {
            var dto = BuildContentDto(entity);
            dto.ContentVersionDto = BuildContentVersionDto(entity, dto);
            return dto;
        }

        private static ContentDto BuildContentDto(IMedia entity)
        {
            var dto = new ContentDto
            {
                // Id = _primaryKey if >0 - fixme - kill that id entirely
                NodeId = entity.Id,
                ContentTypeId = entity.ContentTypeId,

                NodeDto = BuildNodeDto(entity)
            };

            return dto;
        }

        private static NodeDto BuildNodeDto(IMedia entity)
        {
            var dto = new NodeDto
            {
                NodeId = entity.Id,
                UniqueId = entity.Key,
                ParentId = entity.ParentId,
                Level = Convert.ToInt16(entity.Level),
                Path = entity.Path,
                SortOrder = entity.SortOrder,
                Trashed = entity.Trashed,
                UserId = entity.CreatorId,
                Text = entity.Name,
                NodeObjectType = Constants.ObjectTypes.Media,
                CreateDate = entity.CreateDate
            };

            return dto;
        }

        private static ContentVersionDto BuildContentVersionDto(IMedia entity, ContentDto contentDto)
        {
            var dto = new ContentVersionDto
            {
                //Id =, // fixme
                NodeId = entity.Id,
                VersionId = entity.Version,
                VersionDate = entity.UpdateDate,
                Current = true, // always building the current one
                Text = entity.Name,

                ContentDto = contentDto
            };

            return dto;
        }
    }
}
