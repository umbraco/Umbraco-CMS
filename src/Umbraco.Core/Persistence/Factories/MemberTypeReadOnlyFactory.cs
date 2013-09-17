using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MemberTypeReadOnlyFactory : IEntityFactory<IMemberType, MemberTypeReadOnlyDto>
    {
        public IMemberType BuildEntity(MemberTypeReadOnlyDto dto)
        {
            var memberType = new MemberType(dto.ParentId)
                             {
                                 Alias = dto.Alias,
                                 AllowedAsRoot = dto.AllowAtRoot,
                                 CreateDate = dto.CreateDate,
                                 CreatorId = dto.UserId.HasValue ? dto.UserId.Value : 0,
                                 Description = dto.Description,
                                 Icon = dto.Icon,
                                 Id = dto.NodeId,
                                 IsContainer = dto.IsContainer,
                                 Key = dto.UniqueId.Value,
                                 Level = dto.Level,
                                 Name = dto.Text,
                                 Path = dto.Path,
                                 SortOrder = dto.SortOrder,
                                 Thumbnail = dto.Thumbnail,
                                 Trashed = dto.Trashed,
                                 UpdateDate = dto.CreateDate,
                                 AllowedContentTypes = Enumerable.Empty<ContentTypeSort>()
                             };

            var propertyTypeGroupCollection = GetPropertyTypeGroupCollection(dto, memberType);
            memberType.PropertyGroups = propertyTypeGroupCollection;

            var propertyTypes = GetPropertyTypes(dto, memberType);
            //By Convention we add 9 stnd PropertyTypes - This is only here to support loading of types that didn't have these conventions before.
            var standardPropertyTypes = Constants.Conventions.Member.StandardPropertyTypeStubs;
            foreach (var standardPropertyType in standardPropertyTypes)
            {
                if(dto.PropertyTypes.Any(x => x.Alias.Equals(standardPropertyType.Key))) continue;
                
                //Add the standard PropertyType to the current list
                propertyTypes.Add(standardPropertyType.Value);

                //Internal dictionary for adding "MemberCanEdit" and "VisibleOnProfile" properties to each PropertyType
                memberType.MemberTypePropertyTypes.Add(standardPropertyType.Key,
                    new Tuple<bool, bool, int>(false, false, default(int)));
            }
            memberType.PropertyTypes = propertyTypes;

            return memberType;
        }

        private PropertyGroupCollection GetPropertyTypeGroupCollection(MemberTypeReadOnlyDto dto, MemberType memberType)
        {
            var propertyTypeGroupCollection = new PropertyGroupCollection();
            foreach (var propertyTypeGroup in dto.PropertyTypeGroups.Where(x => x.Id.HasValue))
            {
                //Find PropertyTypes that belong to the current PropertyTypeGroup
                var groupId = propertyTypeGroup.Id.Value;
                var propertyTypesByGroup =
                    dto.PropertyTypes.Where(
                        x => x.Id.HasValue && x.PropertyTypeGroupId.HasValue && x.PropertyTypeGroupId.Value.Equals(groupId));
                //Create PropertyTypeCollection for passing into the PropertyTypeGroup, and loop through the above result to create PropertyTypes
                var propertyTypeCollection = new PropertyTypeCollection();
                foreach (var propTypeDto in propertyTypesByGroup)
                {
                    //Internal dictionary for adding "MemberCanEdit" and "VisibleOnProfile" properties to each PropertyType
                    memberType.MemberTypePropertyTypes.Add(propTypeDto.Alias,
                        new Tuple<bool, bool, int>(propTypeDto.CanEdit, propTypeDto.ViewOnProfile, propTypeDto.Id.Value));
                    //PropertyType Collection
                    propertyTypeCollection.Add(new PropertyType(propTypeDto.PropertyEditorAlias,
                        propTypeDto.DbType.EnumParse<DataTypeDatabaseType>(true))
                                               {
                                                   Alias = propTypeDto.Alias,
                                                   DataTypeDefinitionId = propTypeDto.DataTypeId,
                                                   Description = propTypeDto.Description,
                                                   HelpText = propTypeDto.HelpText,
                                                   Id = propTypeDto.Id.Value,
                                                   Mandatory = propTypeDto.Mandatory,
                                                   Name = propTypeDto.Name,
                                                   SortOrder = propTypeDto.SortOrder,
                                                   ValidationRegExp = propTypeDto.ValidationRegExp,
                                                   PropertyGroupId = new Lazy<int>(() => propTypeDto.PropertyTypeGroupId.Value),
                                                   CreateDate = dto.CreateDate,
                                                   UpdateDate = dto.CreateDate
                                               });
                }

                var group = new PropertyGroup(propertyTypeCollection) {Id = groupId};
                propertyTypeGroupCollection.Add(@group);
            }
            return propertyTypeGroupCollection;
        }

        private List<PropertyType> GetPropertyTypes(MemberTypeReadOnlyDto dto, MemberType memberType)
        {
            //Find PropertyTypes that does not belong to a PropertyTypeGroup
            var propertyTypes = new List<PropertyType>();
            foreach (var propertyType in dto.PropertyTypes.Where(x => x.PropertyTypeGroupId.HasValue == false && x.Id.HasValue))
            {
                //Internal dictionary for adding "MemberCanEdit" and "VisibleOnProfile" properties to each PropertyType
                memberType.MemberTypePropertyTypes.Add(propertyType.Alias,
                    new Tuple<bool, bool, int>(propertyType.CanEdit, propertyType.ViewOnProfile, propertyType.Id.Value));
                //PropertyType Collection
                propertyTypes.Add(new PropertyType(propertyType.PropertyEditorAlias,
                    propertyType.DbType.EnumParse<DataTypeDatabaseType>(true))
                                  {
                                      Alias = propertyType.Alias,
                                      DataTypeDefinitionId = propertyType.DataTypeId,
                                      Description = propertyType.Description,
                                      HelpText = propertyType.HelpText,
                                      Id = propertyType.Id.Value,
                                      Mandatory = propertyType.Mandatory,
                                      Name = propertyType.Name,
                                      SortOrder = propertyType.SortOrder,
                                      ValidationRegExp = propertyType.ValidationRegExp,
                                      PropertyGroupId = new Lazy<int>(() => default(int)),
                                      CreateDate = dto.CreateDate,
                                      UpdateDate = dto.CreateDate
                                  });
            }
            return propertyTypes;
        }

        public MemberTypeReadOnlyDto BuildDto(IMemberType entity)
        {
            throw new System.NotImplementedException();
        }
        
    }
}