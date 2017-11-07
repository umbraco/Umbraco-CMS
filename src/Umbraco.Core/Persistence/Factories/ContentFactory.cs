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
            var versionDto = dto.DocumentVersionDto;
            var contentVersionDto = versionDto.ContentVersionDto;

            var content = new Content(nodeDto.Text, nodeDto.ParentId, contentType);

            try
            {
                content.DisableChangeTracking();

                content.Id = dto.NodeId;
                content.Key = nodeDto.UniqueId;
                content.Version = contentVersionDto.VersionId;

                content.Name = nodeDto.Text;
                content.NodeName = nodeDto.Text;

                content.Path = nodeDto.Path;
                content.Level = nodeDto.Level;
                content.ParentId = nodeDto.ParentId;
                content.SortOrder = nodeDto.SortOrder;
                content.Trashed = nodeDto.Trashed;

                content.CreatorId = nodeDto.UserId ?? 0;
                content.WriterId = dto.WriterUserId;
                content.CreateDate = nodeDto.CreateDate;
                content.UpdateDate = contentVersionDto.VersionDate;

                content.Published = dto.Published;
                content.ExpireDate = dto.ExpiresDate;
                content.ReleaseDate = dto.ReleaseDate;

                // if not published, published date has no meaning really
                content.PublishedDate = dto.Published ? contentVersionDto.VersionDate : DateTime.MinValue;

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
        public static DocumentDto BuildDto(IContent entity)
        {
            var contentDto = BuildContentDto(entity);

            var dto = new DocumentDto
            {
                NodeId = entity.Id,
                Published = entity.Published,
                WriterUserId = entity.WriterId,
                ReleaseDate = entity.ReleaseDate,
                ExpiresDate = entity.ExpireDate,
                UpdateDate = entity.UpdateDate,

                ContentDto = contentDto,
                DocumentVersionDto = BuildDocumentVersionDto(entity, contentDto)
            };

            return dto;
        }

        private static ContentDto BuildContentDto(IContent entity)
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

        private static NodeDto BuildNodeDto(IContent entity)
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
                NodeObjectType = Constants.ObjectTypes.Document,
                CreateDate = entity.CreateDate,
            };

            return dto;
        }

        private static DocumentVersionDto BuildDocumentVersionDto(IContent entity, ContentDto contentDto)
        {
            var dto = new DocumentVersionDto
            {
                //Id =, // fixme
                TemplateId = entity.Template?.Id ?? 0,

                ContentVersionDto = BuildContentVersionDto(entity, contentDto)
            };

            return dto;
        }

        private static ContentVersionDto BuildContentVersionDto(IContent entity, ContentDto contentDto)
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
