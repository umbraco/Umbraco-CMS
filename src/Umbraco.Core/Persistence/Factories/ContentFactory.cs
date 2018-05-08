using System;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class ContentFactory
    {
        private readonly IContentType _contentType;
        private readonly Guid _nodeObjectTypeId;
        private readonly int _id;
        private int _primaryKey;

        public ContentFactory(IContentType contentType, Guid nodeObjectTypeId, int id)
        {
            _contentType = contentType;
            _nodeObjectTypeId = nodeObjectTypeId;
            _id = id;
        }

        public ContentFactory(Guid nodeObjectTypeId, int id)
        {
            _nodeObjectTypeId = nodeObjectTypeId;
            _id = id;
        }

        #region Implementation of IEntityFactory<IContent,DocumentDto>

        /// <summary>
        /// Builds a IContent item from the dto(s) and content type
        /// </summary>
        /// <param name="dto">
        /// This DTO can contain all of the information to build an IContent item, however in cases where multiple entities are being built,
        /// a separate <see cref="DocumentPublishedReadOnlyDto"/> publishedDto entity will be supplied in place of the <see cref="DocumentDto"/>'s own 
        /// ResultColumn DocumentPublishedReadOnlyDto
        /// </param>
        /// <param name="contentType"></param>
        /// <param name="publishedDto">
        /// When querying for multiple content items the main DTO will not contain the ResultColumn DocumentPublishedReadOnlyDto and a separate publishedDto instance will be supplied
        /// </param>
        /// <returns></returns>
        public static IContent BuildEntity(DocumentDto dto, IContentType contentType, DocumentPublishedReadOnlyDto publishedDto = null)
        {
            var content = new Content(dto.Text, dto.ContentVersionDto.ContentDto.NodeDto.ParentId, contentType);

            try
            {
                content.DisableChangeTracking();

                content.Id = dto.NodeId;
                content.Key = dto.ContentVersionDto.ContentDto.NodeDto.UniqueId;
                content.Name = dto.Text;
                content.NodeName = dto.ContentVersionDto.ContentDto.NodeDto.Text;
                content.Path = dto.ContentVersionDto.ContentDto.NodeDto.Path;
                content.CreatorId = dto.ContentVersionDto.ContentDto.NodeDto.UserId.Value;
                content.WriterId = dto.WriterUserId;
                content.Level = dto.ContentVersionDto.ContentDto.NodeDto.Level;
                content.ParentId = dto.ContentVersionDto.ContentDto.NodeDto.ParentId;
                content.SortOrder = dto.ContentVersionDto.ContentDto.NodeDto.SortOrder;
                content.Trashed = dto.ContentVersionDto.ContentDto.NodeDto.Trashed;
                content.Published = dto.Published;
                content.CreateDate = dto.ContentVersionDto.ContentDto.NodeDto.CreateDate;
                content.UpdateDate = dto.ContentVersionDto.VersionDate;
                content.ExpireDate = dto.ExpiresDate.HasValue ? dto.ExpiresDate.Value : (DateTime?)null;
                content.ReleaseDate = dto.ReleaseDate.HasValue ? dto.ReleaseDate.Value : (DateTime?)null;
                content.Version = dto.ContentVersionDto.VersionId;

                content.PublishedState = dto.Published ? PublishedState.Published : PublishedState.Unpublished;

                //Check if the publishedDto has been supplied, if not the use the dto's own DocumentPublishedReadOnlyDto value
                content.PublishedVersionGuid = publishedDto == null
                    ? (dto.DocumentPublishedReadOnlyDto == null ? default(Guid) : dto.DocumentPublishedReadOnlyDto.VersionId)
                    : publishedDto.VersionId;
                content.PublishedDate = publishedDto == null
                    ? (dto.DocumentPublishedReadOnlyDto == null ? default(DateTime) : dto.DocumentPublishedReadOnlyDto.VersionDate)
                    : publishedDto.VersionDate;

                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                content.ResetDirtyProperties(false);
                return content;
            }
            finally
            {
                content.EnableChangeTracking();
            }

        }

        [Obsolete("Use the static BuildEntity instead so we don't have to allocate one of these objects everytime we want to map values")]
        public IContent BuildEntity(DocumentDto dto)
        {
            return BuildEntity(dto, _contentType);
        }

        public DocumentDto BuildDto(IContent entity)
        {
            //NOTE Currently doesn't add Alias (legacy that eventually will go away)
            var documentDto = new DocumentDto
            {
                Newest = true,
                NodeId = entity.Id,
                Published = entity.Published,
                Text = entity.Name,
                UpdateDate = entity.UpdateDate,
                WriterUserId = entity.WriterId,
                VersionId = entity.Version,
                ExpiresDate = null,
                ReleaseDate = null,
                ContentVersionDto = BuildContentVersionDto(entity)
            };

            if (entity.Template != null && entity.Template.Id > 0)
                documentDto.TemplateId = entity.Template.Id;

            if (entity.ExpireDate.HasValue)
                documentDto.ExpiresDate = entity.ExpireDate.Value;

            if (entity.ReleaseDate.HasValue)
                documentDto.ReleaseDate = entity.ReleaseDate.Value;

            return documentDto;
        }

        #endregion

        public void SetPrimaryKey(int primaryKey)
        {
            _primaryKey = primaryKey;
        }

        private ContentVersionDto BuildContentVersionDto(IContent entity)
        {
            //TODO: Change this once the Language property is public on IContent
            var content = entity as Content;
            var lang = content == null ? string.Empty : content.Language;

            var contentVersionDto = new ContentVersionDto
            {
                NodeId = entity.Id,
                VersionDate = entity.UpdateDate,
                VersionId = entity.Version,
                ContentDto = BuildContentDto(entity)
            };
            return contentVersionDto;
        }

        private ContentDto BuildContentDto(IContent entity)
        {
            var contentDto = new ContentDto
            {
                NodeId = entity.Id,
                ContentTypeId = entity.ContentTypeId,
                NodeDto = BuildNodeDto(entity)
            };

            if (_primaryKey > 0)
            {
                contentDto.PrimaryKey = _primaryKey;
            }

            return contentDto;
        }

        private NodeDto BuildNodeDto(IContent entity)
        {
            //TODO: Change this once the Language property is public on IContent            
            var nodeName = entity.Name;

            var nodeDto = new NodeDto
            {
                CreateDate = entity.CreateDate,
                NodeId = entity.Id,
                Level = short.Parse(entity.Level.ToString(CultureInfo.InvariantCulture)),
                NodeObjectType = _nodeObjectTypeId,
                ParentId = entity.ParentId,
                Path = entity.Path,
                SortOrder = entity.SortOrder,
                Text = nodeName,
                Trashed = entity.Trashed,
                UniqueId = entity.Key,
                UserId = entity.CreatorId
            };

            return nodeDto;
        }
    }
}