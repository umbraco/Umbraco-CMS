using System;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MediaFactory : IEntityFactory<IMedia, ContentVersionDto>
    {
        private readonly IMediaType _contentType;
        private readonly Guid _nodeObjectTypeId;
        private readonly int _id;
        private int _primaryKey;
        private readonly IProfile _user;

        public MediaFactory(IMediaType contentType, Guid nodeObjectTypeId, int id, IProfile user)
        {
            _contentType = contentType;
            _nodeObjectTypeId = nodeObjectTypeId;
            _id = id;
            _user = user;
        }

        public MediaFactory(Guid nodeObjectTypeId, int id)
        {
            _nodeObjectTypeId = nodeObjectTypeId;
            _id = id;
        }

        #region Implementation of IEntityFactory<IMedia,ContentVersionDto>

        public IMedia BuildEntity(ContentVersionDto dto)
        {
            return new Models.Media(dto.ContentDto.NodeDto.ParentId, _contentType)
                       {
                           Id = _id,
                           Key =
                               dto.ContentDto.NodeDto.UniqueId.HasValue
                                   ? dto.ContentDto.NodeDto.UniqueId.Value
                                   : _id.ToGuid(),
                           Name = dto.ContentDto.NodeDto.Text,
                           Path = dto.ContentDto.NodeDto.Path,
                           Creator = _user,
                           Level = dto.ContentDto.NodeDto.Level,
                           ParentId = dto.ContentDto.NodeDto.ParentId,
                           SortOrder = dto.ContentDto.NodeDto.SortOrder,
                           Trashed = dto.ContentDto.NodeDto.Trashed,
                           CreateDate = dto.ContentDto.NodeDto.CreateDate,
                           UpdateDate = dto.VersionDate,
                           Version = dto.VersionId
                       };
        }

        public ContentVersionDto BuildDto(IMedia entity)
        {
            var dto = new ContentVersionDto
                                        {
                                            NodeId = entity.Id,
                                            VersionDate = entity.UpdateDate,
                                            VersionId = entity.Version,
                                            ContentDto = BuildContentDto(entity)
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
                                  UserId = entity.Creator.Id.SafeCast<int>()
                              };

            return nodeDto;
        }
    }
}