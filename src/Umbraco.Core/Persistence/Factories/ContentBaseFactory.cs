using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class ContentBaseFactory
    {
        /// <summary>
        /// Builds an IContent item from a dto and content type.
        /// </summary>
        public static Content BuildEntity(DocumentDto dto, IContentType contentType)
        {
            var contentDto = dto.ContentDto;
            var nodeDto = contentDto.NodeDto;
            var documentVersionDto = dto.DocumentVersionDto;
            var contentVersionDto = documentVersionDto.ContentVersionDto;
            var publishedVersionDto = dto.PublishedVersionDto;

            var content = new Content(nodeDto.Text, nodeDto.ParentId, contentType);

            try
            {
                content.DisableChangeTracking();

                content.Id = dto.NodeId;
                content.Key = nodeDto.UniqueId;
                content.Version = contentVersionDto.VersionId;
                content.VersionPk = contentVersionDto.Id;

                content.Name = contentVersionDto.Text;
                content.NodeName = contentVersionDto.Text;

                content.Path = nodeDto.Path;
                content.Level = nodeDto.Level;
                content.ParentId = nodeDto.ParentId;
                content.SortOrder = nodeDto.SortOrder;
                content.Trashed = nodeDto.Trashed;

                content.CreatorId = nodeDto.UserId ?? 0;
                content.WriterId = contentVersionDto.UserId;
                content.CreateDate = nodeDto.CreateDate;
                content.UpdateDate = contentVersionDto.VersionDate;

                content.Published = dto.Published;
                content.Edited = dto.Edited;
                content.ExpireDate = dto.ExpiresDate;
                content.ReleaseDate = dto.ReleaseDate;

                // fixme - shall we get published infos or not?
                //if (dto.Published)
                if (publishedVersionDto != null)
                {
                    content.PublishedVersionPk = publishedVersionDto.Id;
                    content.PublishDate = publishedVersionDto.ContentVersionDto.VersionDate;
                    content.PublishName = publishedVersionDto.ContentVersionDto.Text;
                    content.PublisherId = publishedVersionDto.ContentVersionDto.UserId;
                }

                // templates = ignored, managed by the repository

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
                content.WriterId = contentVersionDto.UserId;
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
                content.WriterId = contentVersionDto.UserId;
                content.CreateDate = nodeDto.CreateDate;
                content.UpdateDate = contentVersionDto.VersionDate;

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
        /// Buils a dto from an IContent item.
        /// </summary>
        public static DocumentDto BuildDto(IContent entity, Guid objectType)
        {
            var contentBase = (Content) entity;
            var contentDto = BuildContentDto(contentBase, objectType);

            var dto = new DocumentDto
            {
                NodeId = entity.Id,
                Published = entity.Published,
                ReleaseDate = entity.ReleaseDate,
                ExpiresDate = entity.ExpireDate,

                ContentDto = contentDto,
                DocumentVersionDto = BuildDocumentVersionDto(contentBase, contentDto)
            };

            return dto;
        }

        /// <summary>
        /// Buils a dto from an IMedia item.
        /// </summary>
        public static ContentDto BuildDto(IMedia entity)
        {
            var contentBase = (Models.Media) entity;
            var dto = BuildContentDto(contentBase, Constants.ObjectTypes.Media);
            dto.ContentVersionDto = BuildContentVersionDto(contentBase, dto);
            return dto;
        }

        /// <summary>
        /// Buils a dto from an IMember item.
        /// </summary>
        public static MemberDto BuildDto(IMember entity)
        {
            var member = (Member) entity;
            var contentDto = BuildContentDto(member, Constants.ObjectTypes.Member);

            var dto = new MemberDto
            {
                Email = entity.Email,
                LoginName = entity.Username,
                NodeId = entity.Id,
                Password = entity.RawPasswordValue,

                ContentDto = contentDto,
                ContentVersionDto = BuildContentVersionDto(member, contentDto)
            };
            return dto;
        }

        private static ContentDto BuildContentDto(ContentBase entity, Guid objectType)
        {
            var dto = new ContentDto
            {
                NodeId = entity.Id,
                ContentTypeId = entity.ContentTypeId,

                NodeDto = BuildNodeDto(entity, objectType)
            };

            return dto;
        }

        private static NodeDto BuildNodeDto(ContentBase entity, Guid objectType)
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
        private static ContentVersionDto BuildContentVersionDto(ContentBase entity, ContentDto contentDto)
        {
            var dto = new ContentVersionDto
            {
                Id = entity.VersionPk,
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

        // always build the current / VersionPk dto
        // we're never going to build / save old versions (which are immutable)
        private static DocumentVersionDto BuildDocumentVersionDto(Content entity, ContentDto contentDto)
        {
            var dto = new DocumentVersionDto
            {
                Id = entity.VersionPk,
                TemplateId = entity.Template?.Id,
                Published = false, // always building the current, unpublished one

                ContentVersionDto = BuildContentVersionDto(entity, contentDto)
            };

            return dto;
        }
    }
}
