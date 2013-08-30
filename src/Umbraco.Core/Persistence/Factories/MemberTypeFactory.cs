using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MemberTypeFactory : IEntityFactory<IMemberType, ContentTypeDto>
    {
        private readonly Guid _nodeObjectType;

        public MemberTypeFactory(Guid nodeObjectType)
        {
            _nodeObjectType = nodeObjectType;
        }

        public IMemberType BuildEntity(ContentTypeDto dto)
        {
            throw new System.NotImplementedException();
        }

        public ContentTypeDto BuildDto(IMemberType entity)
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

        public IEnumerable<MemberTypeDto> BuildMemberTypeDtos(IMemberType entity)
        {
            var memberType = entity as MemberType;
            if (memberType == null || memberType.MemberTypePropertyTypes.Any() == false)
                return Enumerable.Empty<MemberTypeDto>();

            return memberType.MemberTypePropertyTypes.Select(propertyType => new MemberTypeDto
                                                                             {
                                                                                 NodeId = entity.Id, 
                                                                                 CanEdit = propertyType.Value.Item1, 
                                                                                 ViewOnProfile = propertyType.Value.Item2, 
                                                                                 PropertyTypeId = propertyType.Value.Item3
                                                                             }).ToList();
        }

        private NodeDto BuildNodeDto(IMemberType entity)
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