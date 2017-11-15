using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MemberFactory
    {
        /// <summary>
        /// Builds an IMedia item from a dto and content type.
        /// </summary>
        public static Member BuildEntity(MemberDto dto, IMemberType contentType)
        {
            var nodeDto = dto.ContentDto.NodeDto;
            var contentVersionDto = dto.ContentVersionDto;

            var content = new Member(nodeDto.Text, dto.Email, dto.LoginName, dto.Password, contentType);

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
                content.WriterId = dto.ContentDto.WriterUserId;
                content.CreateDate = nodeDto.CreateDate;
                content.UpdateDate = dto.ContentDto.UpdateDate;

                content.ProviderUserKey = content.Key; // fixme explain

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
        /// Buils a dto from an IMember item.
        /// </summary>
        public static MemberDto BuildDto(IMember entity)
        {
            var contentDto = BuildContentDto(entity, Constants.ObjectTypes.Member);

            var dto = new MemberDto
            {
                Email = entity.Email,
                LoginName = entity.Username,
                NodeId = entity.Id,
                Password = entity.RawPasswordValue,

                ContentDto = contentDto,
                ContentVersionDto = BuildContentVersionDto(entity, contentDto)
            };
            return dto;
        }

        private static ContentDto BuildContentDto(IMember entity, Guid objectType)
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

        private static NodeDto BuildNodeDto(IMember entity, Guid objectType)
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

        private static ContentVersionDto BuildContentVersionDto(IMember entity, ContentDto contentDto)
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
