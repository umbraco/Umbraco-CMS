using System;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MediaTypeFactory : IEntityFactory<IMediaType, ContentTypeDto>
    {
        private readonly Guid _nodeObjectType;
        private readonly IProfile _creator;

        public MediaTypeFactory(Guid nodeObjectType)
        {
            _nodeObjectType = nodeObjectType;
        }

        public MediaTypeFactory(Guid nodeObjectType, IProfile creator)
        {
            _nodeObjectType = nodeObjectType;
            _creator = creator;
        }

        #region Implementation of IEntityFactory<IMediaType,ContentTypeDto>

        public IMediaType BuildEntity(ContentTypeDto dto)
        {
            var contentType = new MediaType(dto.NodeDto.ParentId)
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
                                      Creator = _creator,
                                      AllowedAsRoot = dto.AllowAtRoot,
                                      IsContainer = dto.IsContainer,
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
                                         AllowAtRoot = entity.AllowedAsRoot,
                                         IsContainer = entity.IsContainer,
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
                                  UserId = entity.Creator.Id.SafeCast<int>()
                              };
            return nodeDto;
        }
    }
}