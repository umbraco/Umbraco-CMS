using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class PropertyGroupFactory
    {
        
        #region Implementation of IEntityFactory<IEnumerable<PropertyGroup>,IEnumerable<TabDto>>

        public static IEnumerable<PropertyGroup> BuildEntity(IEnumerable<PropertyTypeGroupDto> groupDtos,
                                                             bool isPublishing,
                                                             int contentTypeId,
                                                             DateTime createDate,
                                                             DateTime updateDate,
                                                             Func<string, ValueStorageType, string, PropertyType> propertyTypeCtor)
        {
            // groupDtos contains all the groups, those that are defined on the current
            // content type, and those that are inherited from composition content types
            var propertyGroups = new PropertyGroupCollection();
            foreach (var groupDto in groupDtos)
            {
                var group = new PropertyGroup(isPublishing);

                try
                {
                    group.DisableChangeTracking();

                    // if the group is defined on the current content type,
                    // assign its identifier, else it will be zero
                    if (groupDto.ContentTypeNodeId == contentTypeId)
                        group.Id = groupDto.Id;

                    group.Name = groupDto.Text;
                    group.SortOrder = groupDto.SortOrder;
                    group.PropertyTypes = new PropertyTypeCollection(isPublishing);
                    group.Key = groupDto.UniqueId;

                    //Because we are likely to have a group with no PropertyTypes we need to ensure that these are excluded
                    var typeDtos = groupDto.PropertyTypeDtos.Where(x => x.Id > 0);
                    foreach (var typeDto in typeDtos)
                    {
                        var tempGroupDto = groupDto;
                        var propertyType = propertyTypeCtor(typeDto.DataTypeDto.EditorAlias,
                            typeDto.DataTypeDto.DbType.EnumParse<ValueStorageType>(true),
                            typeDto.Alias);

                        try
                        {
                            propertyType.DisableChangeTracking();

                            propertyType.Alias = typeDto.Alias;
                            propertyType.DataTypeId = typeDto.DataTypeId;
                            propertyType.Description = typeDto.Description;
                            propertyType.Id = typeDto.Id;
                            propertyType.Key = typeDto.UniqueId;
                            propertyType.Name = typeDto.Name;
                            propertyType.Mandatory = typeDto.Mandatory;
                            propertyType.SortOrder = typeDto.SortOrder;
                            propertyType.ValidationRegExp = typeDto.ValidationRegExp;
                            propertyType.PropertyGroupId = new Lazy<int>(() => tempGroupDto.Id);
                            propertyType.CreateDate = createDate;
                            propertyType.UpdateDate = updateDate;
                            propertyType.Variations = (ContentVariation)typeDto.Variations;

                            // reset dirty initial properties (U4-1946)
                            propertyType.ResetDirtyProperties(false);
                            group.PropertyTypes.Add(propertyType);
                        }
                        finally
                        {
                            propertyType.EnableChangeTracking();
                        }
                    }

                    // reset dirty initial properties (U4-1946)
                    group.ResetDirtyProperties(false);
                    propertyGroups.Add(group);
                }
                finally
                {
                    group.EnableChangeTracking();
                }
            }

            return propertyGroups;
        }

        public static IEnumerable<PropertyTypeGroupDto> BuildDto(IEnumerable<PropertyGroup> entity)
        {
            return entity.Select(BuildGroupDto).ToList();
        }

        #endregion

        internal static PropertyTypeGroupDto BuildGroupDto(PropertyGroup propertyGroup, int contentTypeId)
        {
            var dto = new PropertyTypeGroupDto
            {
                ContentTypeNodeId = contentTypeId,
                SortOrder = propertyGroup.SortOrder,
                Text = propertyGroup.Name,
                UniqueId = propertyGroup.Key
            };

            if (propertyGroup.HasIdentity)
                dto.Id = propertyGroup.Id;

            dto.PropertyTypeDtos = propertyGroup.PropertyTypes.Select(propertyType => BuildPropertyTypeDto(propertyGroup.Id, propertyType, contentTypeId)).ToList();

            return dto;
        }

        internal static PropertyTypeDto BuildPropertyTypeDto(int tabId, PropertyType propertyType, int contentTypeId)
        {
            var propertyTypeDto = new PropertyTypeDto
            {
                Alias = propertyType.Alias,
                ContentTypeId = contentTypeId,
                DataTypeId = propertyType.DataTypeId,
                Description = propertyType.Description,
                Mandatory = propertyType.Mandatory,
                Name = propertyType.Name,
                SortOrder = propertyType.SortOrder,
                ValidationRegExp = propertyType.ValidationRegExp,
                UniqueId = propertyType.Key,
                Variations = (byte)propertyType.Variations
            };

            if (tabId != default)
            {
                propertyTypeDto.PropertyTypeGroupId = tabId;
            }
            else
            {
                propertyTypeDto.PropertyTypeGroupId = null;
            }

            if (propertyType.HasIdentity)
                propertyTypeDto.Id = propertyType.Id;

            return propertyTypeDto;
        }
    }
}
