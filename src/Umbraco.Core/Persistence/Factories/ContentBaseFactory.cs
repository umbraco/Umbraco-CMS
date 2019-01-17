using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Core.Persistence.Factories
{
    internal class ContentBaseFactory
    {
        private static readonly Regex MediaPathPattern = new Regex(@"(/media/.+?)(?:['""]|$)", RegexOptions.Compiled);

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

                content.CreatorId = nodeDto.UserId ?? Constants.Security.UnknownUserId;
                content.WriterId = contentVersionDto.UserId ?? Constants.Security.UnknownUserId;
                content.CreateDate = nodeDto.CreateDate;
                content.UpdateDate = contentVersionDto.VersionDate;

                content.Published = dto.Published;
                content.Edited = dto.Edited;

                // fixme - shall we get published infos or not?
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
                content.VersionId = contentVersionDto.Id;

                // fixme missing names?

                content.Path = nodeDto.Path;
                content.Level = nodeDto.Level;
                content.ParentId = nodeDto.ParentId;
                content.SortOrder = nodeDto.SortOrder;
                content.Trashed = nodeDto.Trashed;

                content.CreatorId = nodeDto.UserId ?? Constants.Security.UnknownUserId;
                content.WriterId = contentVersionDto.UserId ?? Constants.Security.UnknownUserId;
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
                content.VersionId = contentVersionDto.Id;

                // fixme missing names?

                content.Path = nodeDto.Path;
                content.Level = nodeDto.Level;
                content.ParentId = nodeDto.ParentId;
                content.SortOrder = nodeDto.SortOrder;
                content.Trashed = nodeDto.Trashed;

                content.CreatorId = nodeDto.UserId ?? Constants.Security.UnknownUserId;
                content.WriterId = contentVersionDto.UserId ?? Constants.Security.UnknownUserId;
                content.CreateDate = nodeDto.CreateDate;
                content.UpdateDate = contentVersionDto.VersionDate;

                content.ProviderUserKey = content.Key; // The `ProviderUserKey` is a membership provider thing

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
        public static MediaDto BuildDto(IMedia entity)
        {
            var contentDto = BuildContentDto(entity, Constants.ObjectTypes.Media);

            var dto = new MediaDto
            {
                NodeId = entity.Id,
                ContentDto = contentDto,
                MediaVersionDto = BuildMediaVersionDto(entity, contentDto)
            };

            return dto;
        }

        /// <summary>
        /// Builds a dto from an IMember item.
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

        private static MediaVersionDto BuildMediaVersionDto(IMedia entity, ContentDto contentDto)
        {
            // try to get a path from the string being stored for media
            // fixme - only considering umbracoFile ?!

            TryMatch(entity.GetValue<string>("umbracoFile"), out var path);

            var dto = new MediaVersionDto
            {
                Id = entity.VersionId,
                Path = path,

                ContentVersionDto = BuildContentVersionDto(entity, contentDto)
            };

            return dto;
        }

        // fixme - this should NOT be here?!
        // more dark magic ;-(
        internal static bool TryMatch(string text, out string path)
        {
            //fixme: In v8 we should allow exposing this via the property editor in a much nicer way so that the property editor
            // can tell us directly what any URL is for a given property if it contains an asset

            path = null;
            if (string.IsNullOrWhiteSpace(text)) return false;

            var m = MediaPathPattern.Match(text);
            if (!m.Success || m.Groups.Count != 2) return false;

            path = m.Groups[1].Value;
            return true;
        }
    }
}
