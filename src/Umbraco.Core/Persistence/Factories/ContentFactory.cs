using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class ContentFactory
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
                content.WriterId = contentDto.WriterUserId;
                content.CreateDate = nodeDto.CreateDate;
                content.UpdateDate = contentDto.UpdateDate;

                content.Published = dto.Published;
                content.Edited = dto.Edited;
                content.ExpireDate = dto.ExpiresDate;
                content.ReleaseDate = dto.ReleaseDate;

                if (dto.Published)
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
        /// Buils a dto from an IContent item.
        /// </summary>
        public static DocumentDto BuildDto(IContent entity, Guid objectType)
        {
            var contentDto = BuildContentDto(entity, objectType);

            var dto = new DocumentDto
            {
                NodeId = entity.Id,
                Published = entity.Published,
                ReleaseDate = entity.ReleaseDate,
                ExpiresDate = entity.ExpireDate,

                ContentDto = contentDto,
                DocumentVersionDto = BuildDocumentVersionDto(entity, contentDto)
            };

            return dto;
        }

        private static ContentDto BuildContentDto(IContent entity, Guid objectType)
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

        private static NodeDto BuildNodeDto(IContent entity, Guid objectType)
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
        private static DocumentVersionDto BuildDocumentVersionDto(IContent entity, ContentDto contentDto)
        {
            var dto = new DocumentVersionDto
            {
                Id = ((ContentBase) entity).VersionPk,
                TemplateId = entity.Template?.Id,
                Published = entity.Published,

                ContentVersionDto = BuildContentVersionDto(entity, contentDto)
            };

            return dto;
        }

        // always build the current / VersionPk dto
        // we're never going to build / save old versions (which are immutable)
        private static ContentVersionDto BuildContentVersionDto(IContent entity, ContentDto contentDto)
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
