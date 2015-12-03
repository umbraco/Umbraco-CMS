using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    // factory for
    // IContentType (document types)
    // IMediaType (media types)
    // IMemberType (member types)
    //
    internal class ContentTypeFactory 
    {
        #region IContentType

        public IContentType BuildContentTypeEntity(ContentTypeDto dto)
        {
            var contentType = new ContentType(dto.NodeDto.ParentId);
            BuildCommonEntity(contentType, dto);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            contentType.ResetDirtyProperties(false);

            return contentType;
        }

        #endregion

        #region IMediaType

        public IMediaType BuildMediaTypeEntity(ContentTypeDto dto)
        {
            var contentType = new MediaType(dto.NodeDto.ParentId);
            BuildCommonEntity(contentType, dto);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            contentType.ResetDirtyProperties(false);

            return contentType;
        }

        #endregion

        #region IMemberType

        public IMemberType BuildMemberTypeEntity(ContentTypeDto dto)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MemberTypeDto> BuildMemberTypeDtos(IMemberType entity)
        {
            var memberType = entity as MemberType;
            if (memberType == null || memberType.PropertyTypes.Any() == false)
                return Enumerable.Empty<MemberTypeDto>();

            var dtos = memberType.PropertyTypes.Select(x => new MemberTypeDto
            {
                NodeId = entity.Id,
                PropertyTypeId = x.Id,
                CanEdit = memberType.MemberCanEditProperty(x.Alias),
                ViewOnProfile = memberType.MemberCanViewProperty(x.Alias)
            }).ToList();
            return dtos;
        }

        #endregion

        #region Common

        private static void BuildCommonEntity(ContentTypeBase entity, ContentTypeDto dto)
        {
            entity.Id = dto.NodeDto.NodeId;
            entity.Key = dto.NodeDto.UniqueId;
            entity.Alias = dto.Alias;
            entity.Name = dto.NodeDto.Text;
            entity.Icon = dto.Icon;
            entity.Thumbnail = dto.Thumbnail;
            entity.SortOrder = dto.NodeDto.SortOrder;
            entity.Description = dto.Description;
            entity.CreateDate = dto.NodeDto.CreateDate;
            entity.Path = dto.NodeDto.Path;
            entity.Level = dto.NodeDto.Level;
            entity.CreatorId = dto.NodeDto.UserId.Value;
            entity.AllowedAsRoot = dto.AllowAtRoot;
            entity.IsContainer = dto.IsContainer;
            entity.Trashed = dto.NodeDto.Trashed;
        }

        public ContentTypeDto BuildContentTypeDto(IContentTypeBase entity)
        {
            Guid nodeObjectType;
            if (entity is IContentType)
                nodeObjectType = Constants.ObjectTypes.DocumentTypeGuid;
            else if (entity is IMediaType)
                nodeObjectType = Constants.ObjectTypes.MediaTypeGuid;
            else if (entity is IMemberType)
                nodeObjectType = Constants.ObjectTypes.MemberTypeGuid;
            else
                throw new Exception("oops: invalid entity.");

            var contentTypeDto = new ContentTypeDto
            {
                Alias = entity.Alias,
                Description = entity.Description,
                Icon = entity.Icon,
                Thumbnail = entity.Thumbnail,
                NodeId = entity.Id,
                AllowAtRoot = entity.AllowedAsRoot,
                IsContainer = entity.IsContainer,
                NodeDto = BuildNodeDto(entity, nodeObjectType)
            };
            return contentTypeDto;
        }

        private static NodeDto BuildNodeDto(IUmbracoEntity entity, Guid nodeObjectType)
        {
            var nodeDto = new NodeDto
            {
                CreateDate = entity.CreateDate,
                NodeId = entity.Id,
                Level = short.Parse(entity.Level.ToString(CultureInfo.InvariantCulture)),
                NodeObjectType = nodeObjectType,
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

        #endregion
    }
}