using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories
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
                content.VersionId = contentVersionDto.Id;

                content.Name = contentVersionDto.Text;

                content.Path = nodeDto.Path;
                content.Level = nodeDto.Level;
                content.ParentId = nodeDto.ParentId;
                content.SortOrder = nodeDto.SortOrder;
                content.Trashed = nodeDto.Trashed;

                content.CreatorId = nodeDto.UserId ?? Cms.Core.Constants.Security.UnknownUserId;
                content.WriterId = contentVersionDto.UserId ?? Cms.Core.Constants.Security.UnknownUserId;
                content.CreateDate = nodeDto.CreateDate;
                content.UpdateDate = contentVersionDto.VersionDate;

                content.Published = dto.Published;
                content.Edited = dto.Edited;

                // TODO: shall we get published infos or not?
                //if (dto.Published)
                if (publishedVersionDto != null)
                {
                    content.PublishedVersionId = publishedVersionDto.Id;
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
        public static Core.Models.Media BuildEntity(ContentDto dto, IMediaType contentType)
        {
            var nodeDto = dto.NodeDto;
            var contentVersionDto = dto.ContentVersionDto;

            var content = new Core.Models.Media(nodeDto.Text, nodeDto.ParentId, contentType);

            try
            {
                content.DisableChangeTracking();

                content.Id = dto.NodeId;
                content.Key = nodeDto.UniqueId;
                content.VersionId = contentVersionDto.Id;

                // TODO: missing names?

                content.Path = nodeDto.Path;
                content.Level = nodeDto.Level;
                content.ParentId = nodeDto.ParentId;
                content.SortOrder = nodeDto.SortOrder;
                content.Trashed = nodeDto.Trashed;

                content.CreatorId = nodeDto.UserId ?? Cms.Core.Constants.Security.UnknownUserId;
                content.WriterId = contentVersionDto.UserId ?? Cms.Core.Constants.Security.UnknownUserId;
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
                content.SecurityStamp = dto.SecurityStampToken;
                content.EmailConfirmedDate = dto.EmailConfirmedDate;
                content.PasswordConfiguration = dto.PasswordConfig;
                content.Key = nodeDto.UniqueId;
                content.VersionId = contentVersionDto.Id;
                
                // TODO: missing names?

                content.Path = nodeDto.Path;
                content.Level = nodeDto.Level;
                content.ParentId = nodeDto.ParentId;
                content.SortOrder = nodeDto.SortOrder;
                content.Trashed = nodeDto.Trashed;

                content.CreatorId = nodeDto.UserId ?? Cms.Core.Constants.Security.UnknownUserId;
                content.WriterId = contentVersionDto.UserId ?? Cms.Core.Constants.Security.UnknownUserId;
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
        /// Builds a dto from an IContent item.
        /// </summary>
        public static DocumentDto BuildDto(IContent entity, Guid objectType)
        {
            var contentDto = BuildContentDto(entity, objectType);

            var dto = new DocumentDto
            {
                NodeId = entity.Id,
                Published = entity.Published,
                ContentDto = contentDto,
                DocumentVersionDto = BuildDocumentVersionDto(entity, contentDto)
            };

            return dto;
        }

        public static IEnumerable<(ContentSchedule Model, ContentScheduleDto Dto)> BuildScheduleDto(IContent entity, ILanguageRepository languageRepository)
        {
            return entity.ContentSchedule.FullSchedule.Select(x =>
                (x, new ContentScheduleDto
                {
                    Action = x.Action.ToString(),
                    Date = x.Date,
                    NodeId = entity.Id,
                    LanguageId = languageRepository.GetIdByIsoCode(x.Culture, false),
                    Id = x.Id
                }));
        }

        /// <summary>
        /// Builds a dto from an IMedia item.
        /// </summary>
        public static MediaDto BuildDto(MediaUrlGeneratorCollection mediaUrlGenerators, IMedia entity)
        {
            var contentDto = BuildContentDto(entity, Cms.Core.Constants.ObjectTypes.Media);

            var dto = new MediaDto
            {
                NodeId = entity.Id,
                ContentDto = contentDto,
                MediaVersionDto = BuildMediaVersionDto(mediaUrlGenerators, entity, contentDto)
            };

            return dto;
        }

        /// <summary>
        /// Builds a dto from an IMember item.
        /// </summary>
        public static MemberDto BuildDto(IMember entity)
        {
            var contentDto = BuildContentDto(entity, Cms.Core.Constants.ObjectTypes.Member);

            var dto = new MemberDto
            {
                Email = entity.Email,
                LoginName = entity.Username,
                NodeId = entity.Id,
                Password = entity.RawPasswordValue,
                SecurityStampToken = entity.SecurityStamp,
                EmailConfirmedDate = entity.EmailConfirmedDate,
                ContentDto = contentDto,
                ContentVersionDto = BuildContentVersionDto(entity, contentDto),
                PasswordConfig = entity.PasswordConfiguration
            };
            return dto;
        }

        private static ContentDto BuildContentDto(IContentBase entity, Guid objectType)
        {
            var dto = new ContentDto
            {
                NodeId = entity.Id,
                ContentTypeId = entity.ContentTypeId,

                NodeDto = BuildNodeDto(entity, objectType)
            };

            return dto;
        }

        private static NodeDto BuildNodeDto(IContentBase entity, Guid objectType)
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
        private static ContentVersionDto BuildContentVersionDto(IContentBase entity, ContentDto contentDto)
        {
            var dto = new ContentVersionDto
            {
                Id = entity.VersionId,
                NodeId = entity.Id,
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
        private static DocumentVersionDto BuildDocumentVersionDto(IContent entity, ContentDto contentDto)
        {
            var dto = new DocumentVersionDto
            {
                Id = entity.VersionId,
                TemplateId = entity.TemplateId,
                Published = false, // always building the current, unpublished one

                ContentVersionDto = BuildContentVersionDto(entity, contentDto)
            };

            return dto;
        }

        private static MediaVersionDto BuildMediaVersionDto(MediaUrlGeneratorCollection mediaUrlGenerators, IMedia entity, ContentDto contentDto)
        {
            // try to get a path from the string being stored for media
            // TODO: only considering umbracoFile

            string path = null;

            if (entity.Properties.TryGetValue(Cms.Core.Constants.Conventions.Media.File, out var property)
                && mediaUrlGenerators.TryGetMediaPath(property.PropertyType.PropertyEditorAlias, property.GetValue(), out var mediaPath))
            {
                path = mediaPath;
            }

            var dto = new MediaVersionDto
            {
                Id = entity.VersionId,
                Path = path,

                ContentVersionDto = BuildContentVersionDto(entity, contentDto)
            };

            return dto;
        }
    }
}
