using System;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class ContentTypeFactory : IEntityFactory<IContentType, DocumentTypeDto>
    {
        private readonly Guid _nodeObjectType;

        public ContentTypeFactory(Guid nodeObjectType)
        {
            _nodeObjectType = nodeObjectType;
        }

        #region Implementation of IEntityFactory<IContentType,DocumentTypeDto>

        public IContentType BuildEntity(DocumentTypeDto dto)
        {
            var contentType = new ContentType
                                  {
                                      Id = dto.ContentTypeDto.NodeDto.NodeId,
                                      Key =
                                          dto.ContentTypeDto.NodeDto.UniqueId.HasValue
                                              ? dto.ContentTypeDto.NodeDto.UniqueId.Value
                                              : dto.ContentTypeDto.NodeDto.NodeId.ToGuid(),
                                      Alias = dto.ContentTypeDto.Alias,
                                      Name = dto.ContentTypeDto.NodeDto.Text,
                                      Icon = dto.ContentTypeDto.Icon,
                                      Thumbnail = dto.ContentTypeDto.Thumbnail,
                                      SortOrder = dto.ContentTypeDto.NodeDto.SortOrder,
                                      Description = dto.ContentTypeDto.Description,
                                      CreateDate = dto.ContentTypeDto.NodeDto.CreateDate,
                                      Path = dto.ContentTypeDto.NodeDto.Path,
                                      Level = dto.ContentTypeDto.NodeDto.Level,
                                      ParentId = dto.ContentTypeDto.NodeDto.ParentId,
                                      UserId =
                                          dto.ContentTypeDto.NodeDto.UserId.HasValue
                                              ? dto.ContentTypeDto.NodeDto.UserId.Value
                                              : 0,
                                      Trashed = dto.ContentTypeDto.NodeDto.Trashed
                                  };
            return contentType;
        }

        public DocumentTypeDto BuildDto(IContentType entity)
        {
            var documentTypeDto = new DocumentTypeDto
                                      {ContentTypeDto = BuildContentTypeDto(entity), ContentTypeNodeId = entity.Id};
            //NOTE TemplateId and IsDefault currently not added
            return documentTypeDto;
        }

        #endregion

        private ContentTypeDto BuildContentTypeDto(IContentType entity)
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

        private NodeDto BuildNodeDto(IContentType entity)
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