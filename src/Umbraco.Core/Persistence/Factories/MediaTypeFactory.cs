using System;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MediaTypeFactory : IEntityFactory<IMediaType, ContentTypeDto>
    {
        private readonly Guid _nodeObjectType;

        public MediaTypeFactory(Guid nodeObjectType)
        {
            _nodeObjectType = nodeObjectType;
        }

        #region Implementation of IEntityFactory<IMediaType,ContentTypeDto>

        public IMediaType BuildEntity(ContentTypeDto dto)
        {
            var contentType = new MediaType
                                  {
                                      Id = dto.NodeDto.NodeId,
                                      Key =
                                          dto.NodeDto.UniqueId.HasValue
                                              ? dto.NodeDto.UniqueId.Value
                                              : dto.NodeDto.NodeId.ToGuid(),
                                      Alias = dto.Alias,
                                      Name = dto.NodeDto.Text,
                                      Icon = dto.Icon,
                                      Thumbnail = dto.Thumbnail,
                                      SortOrder = dto.NodeDto.SortOrder,
                                      Description = dto.Description,
                                      CreateDate = dto.NodeDto.CreateDate,
                                      Path = dto.NodeDto.Path,
                                      Level = dto.NodeDto.Level,
                                      ParentId = dto.NodeDto.ParentId,
                                      UserId =
                                          dto.NodeDto.UserId.HasValue
                                              ? dto.NodeDto.UserId.Value
                                              : 0,
                                      Trashed = dto.NodeDto.Trashed
                                  };
            return contentType;
        }

        public ContentTypeDto BuildDto(IMediaType entity)
        {
            var contentTypeDto = new ContentTypeDto
                                     {
                                         Alias = entity.Alias,
                                         Description = entity.Description,
                                         Icon = entity.Icon,
                                         Thumbnail = entity.Thumbnail,
                                         NodeId = entity.Id,
                                         NodeDto = BuildNodeDto(entity)
                                     };
            return contentTypeDto;
        }

        #endregion

        private NodeDto BuildNodeDto(IMediaType entity)
        {
            var nodeDto = new NodeDto
                              {
                                  CreateDate = entity.CreateDate,
                                  NodeId = entity.Id,
                                  Level = short.Parse(entity.Level.ToString(CultureInfo.InvariantCulture)),
                                  NodeObjectType = _nodeObjectType,
                                  ParentId = entity.ParentId,
                                  Path = entity.Path,
                                  SortOrder = entity.SortOrder,
                                  Text = entity.Name,
                                  Trashed = false,
                                  UniqueId = entity.Key,
                                  UserId = entity.UserId
                              };
            return nodeDto;
        }
    }
}