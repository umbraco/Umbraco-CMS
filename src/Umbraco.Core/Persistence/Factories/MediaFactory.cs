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
                content.VersionPk = contentVersionDto.Id;

                // fixme missing names?

                content.Path = nodeDto.Path;
                content.Level = nodeDto.Level;
                content.ParentId = nodeDto.ParentId;
                content.SortOrder = nodeDto.SortOrder;
                content.Trashed = nodeDto.Trashed;

                content.CreatorId = nodeDto.UserId ?? 0;
                content.WriterId = dto.WriterUserId;
                content.CreateDate = nodeDto.CreateDate;
                content.UpdateDate = dto.UpdateDate;

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
            var dto = BuildContentDto(entity, Constants.ObjectTypes.Media);
            dto.ContentVersionDto = BuildContentVersionDto(entity, dto);
            return dto;
        }

        private static ContentDto BuildContentDto(IMedia entity, Guid objectType)
        {
            var dto = new ContentDto
            {
                NodeId = entity.Id,
                ContentTypeId = entity.ContentTypeId,
                WriterUserId = entity.WriterId,
                UpdateDate = entity.UpdateDate,

                NodeDto = BuildNodeDto(entity, objectType)
            };

            return dto;
        }

        private static NodeDto BuildNodeDto(IMedia entity, Guid objectType)
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
                NodeObjectType = objectType,
                CreateDate = entity.CreateDate
            };

            return dto;
        }

        // always build the current / VersionPk dto
        // we're never going to build / save old versions (which are immutable)
        private static ContentVersionDto BuildContentVersionDto(IMedia entity, ContentDto contentDto)
        {
            var dto = new ContentVersionDto
            {
                Id = ((ContentBase) entity).VersionPk,
                NodeId = entity.Id,
                VersionId = entity.Version,
                VersionDate = entity.UpdateDate,
                UserId = entity.WriterId,
                Current = true, // always building the current one
                Text = entity.Name,

                ContentDto = contentDto
            };

            return dto;
        }
    }
}
