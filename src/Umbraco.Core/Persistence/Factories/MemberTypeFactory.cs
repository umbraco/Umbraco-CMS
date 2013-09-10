using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models;
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
            if (memberType == null ||  memberType.PropertyTypes.Any() == false)
                return Enumerable.Empty<MemberTypeDto>();

            var memberTypes = new List<MemberTypeDto>();
            foreach (var propertyType in memberType.PropertyTypes)
            {
                memberTypes.Add(new MemberTypeDto
                                {
                                    NodeId = entity.Id,
                                    PropertyTypeId = propertyType.Id,
                                    CanEdit = memberType.MemberCanEditProperty(propertyType.Alias),
                                    ViewOnProfile = memberType.MemberCanViewProperty(propertyType.Alias)
                                });
            }

            return memberTypes;
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

        private int DeterminePropertyTypeId(int initialId, string alias, IEnumerable<PropertyType> propertyTypes)
        {
            if (initialId == 0 || initialId == default(int))
            {
                var propertyType = propertyTypes.SingleOrDefault(x => x.Alias.Equals(alias));
                if (propertyType == null)
                    return default(int);

                return propertyType.Id;
            }

            return initialId;
        }
    }
}