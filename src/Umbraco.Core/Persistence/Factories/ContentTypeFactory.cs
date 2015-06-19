using System;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class ContentTypeFactory 
    {
        private readonly Guid _nodeObjectType;

        public ContentTypeFactory(Guid nodeObjectType)
        {
            _nodeObjectType = nodeObjectType;
        }

        #region Implementation of IEntityFactory<IContentType,DocumentTypeDto>

        public IContentType BuildEntity(DocumentTypeDto dto)
        {
            var contentType = new ContentType(dto.ContentTypeDto.NodeDto.ParentId)
                                  {
                                      Id = dto.ContentTypeDto.NodeDto.NodeId,
                                      Key = dto.ContentTypeDto.NodeDto.UniqueId,
                                      Alias = dto.ContentTypeDto.Alias,
                                      Name = dto.ContentTypeDto.NodeDto.Text,
                                      Icon = dto.ContentTypeDto.Icon,
                                      Thumbnail = dto.ContentTypeDto.Thumbnail,
                                      SortOrder = dto.ContentTypeDto.NodeDto.SortOrder,
                                      Description = dto.ContentTypeDto.Description,
                                      CreateDate = dto.ContentTypeDto.NodeDto.CreateDate,
                                      Path = dto.ContentTypeDto.NodeDto.Path,
                                      Level = dto.ContentTypeDto.NodeDto.Level,
                                      CreatorId = dto.ContentTypeDto.NodeDto.UserId.Value,
                                      AllowedAsRoot = dto.ContentTypeDto.AllowAtRoot,
                                      IsContainer = dto.ContentTypeDto.IsContainer,
                                      Trashed = dto.ContentTypeDto.NodeDto.Trashed,
                                      DefaultTemplateId = dto.TemplateNodeId
                                  };
            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            contentType.ResetDirtyProperties(false);
            return contentType;
        }

        public DocumentTypeDto BuildDto(IContentType entity)
        {
            var documentTypeDto = new DocumentTypeDto
                                      {ContentTypeDto = BuildContentTypeDto(entity), ContentTypeNodeId = entity.Id};
            
            var contentType = entity as ContentType;
            if(contentType != null)
            {
                documentTypeDto.TemplateNodeId = contentType.DefaultTemplateId;
                documentTypeDto.IsDefault = true;
            }
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
                                         AllowAtRoot = entity.AllowedAsRoot,
                                         IsContainer = entity.IsContainer,
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
                                  UserId = entity.CreatorId
                              };
            return nodeDto;
        }
    }
}