using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MediaFactory 
    {
        private readonly IMediaType _contentType;
        private readonly Guid _nodeObjectTypeId;
        private int _primaryKey;

        public MediaFactory(IMediaType contentType, Guid nodeObjectTypeId)
        {
            _contentType = contentType;
            _nodeObjectTypeId = nodeObjectTypeId;
        }

        public MediaFactory(Guid nodeObjectTypeId)
        {
            _nodeObjectTypeId = nodeObjectTypeId;
        }

        #region Implementation of IEntityFactory<IMedia,ContentVersionDto>

        public static IMedia BuildEntity(ContentVersionDto dto, IMediaType contentType)
        {
            var media = new Models.Media(dto.ContentDto.NodeDto.Text, dto.ContentDto.NodeDto.ParentId, contentType);

            try
            {
                media.DisableChangeTracking();

                media.Id = dto.NodeId;
                media.Key = dto.ContentDto.NodeDto.UniqueId;
                media.Path = dto.ContentDto.NodeDto.Path;
                media.CreatorId = dto.ContentDto.NodeDto.UserId.Value;
                media.Level = dto.ContentDto.NodeDto.Level;
                media.ParentId = dto.ContentDto.NodeDto.ParentId;
                media.SortOrder = dto.ContentDto.NodeDto.SortOrder;
                media.Trashed = dto.ContentDto.NodeDto.Trashed;
                media.CreateDate = dto.ContentDto.NodeDto.CreateDate;
                media.UpdateDate = dto.VersionDate;
                media.Version = dto.VersionId;
                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                media.ResetDirtyProperties(false);
                return media;
            }
            finally
            {
                media.EnableChangeTracking();
            }

        }

        [Obsolete("Use the static BuildEntity instead so we don't have to allocate one of these objects everytime we want to map values")]
        public IMedia BuildEntity(ContentVersionDto dto)
        {
            return BuildEntity(dto, _contentType);
        }

        public MediaDto BuildDto(IMedia entity)
        {
            var versionDto = new ContentVersionDto
            {
                NodeId = entity.Id,
                VersionDate = entity.UpdateDate,
                VersionId = entity.Version,
                ContentDto = BuildContentDto(entity)
            };

            var dto = new MediaDto()
            {
                NodeId = entity.Id,
                ContentVersionDto = versionDto,
                MediaPath = GetMediaPath(entity),
                VersionId = entity.Version
            };
            return dto;
        }

        #endregion

        public void SetPrimaryKey(int primaryKey)
        {
            _primaryKey = primaryKey;
        }

        private ContentDto BuildContentDto(IMedia entity)
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

        private NodeDto BuildNodeDto(IMedia entity)
        {
            var nodeDto = new NodeDto
                              {
                                  CreateDate = entity.CreateDate,
                                  NodeId = entity.Id,
                                  Level = short.Parse(entity.Level.ToString(CultureInfo.InvariantCulture)),
                                  NodeObjectType = _nodeObjectTypeId,
                                  ParentId = entity.ParentId,
                                  Path = entity.Path,
                                  SortOrder = entity.SortOrder,
                                  Text = entity.Name,
                                  Trashed = entity.Trashed,
                                  UniqueId = entity.Key,
                                  UserId = entity.CreatorId
                              };

            return nodeDto;
        }

        private static readonly Regex SrcPathPattern = new Regex(@"['""]src['""]:\s*['""](.+?)['""]", RegexOptions.Compiled);

        internal static string GetMediaPath(IMedia entity)
        {
            foreach( var property in entity.Properties)
            {
                if ( property.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias)
                {
                    return entity.GetValue<string>(property.Alias);
                }
                else if ( property.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.ImageCropperAlias)
                {
                    string value = entity.GetValue<string>(property.Alias);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        var match = SrcPathPattern.Match(value);
                        if (match.Success)
                            return match.Groups[1].Value;
                    }                    
                }
            }

            return null;
        }
    }
}
