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

                    group.Key = groupDto.UniqueId;
                    group.Type = (PropertyGroupType)groupDto.Type;
                    group.Name = groupDto.Text;
                    group.Alias = groupDto.Alias;
                    group.SortOrder = groupDto.SortOrder;

                    group.PropertyTypes = new PropertyTypeCollection(isPublishing);

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
                            propertyType.DataTypeKey = typeDto.DataTypeDto.NodeDto.UniqueId;
                            propertyType.Description = typeDto.Description;
                            propertyType.Id = typeDto.Id;
                            propertyType.Key = typeDto.UniqueId;
                            propertyType.Name = typeDto.Name;
                            propertyType.Mandatory = typeDto.Mandatory;
                            propertyType.MandatoryMessage = typeDto.MandatoryMessage;
                            propertyType.SortOrder = typeDto.SortOrder;
                            propertyType.ValidationRegExp = typeDto.ValidationRegExp;
                            propertyType.ValidationRegExpMessage = typeDto.ValidationRegExpMessage;
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
                UniqueId = propertyGroup.Key,
                Type = (short)propertyGroup.Type,
                ContentTypeNodeId = contentTypeId,
                Text = propertyGroup.Name,
                Alias = propertyGroup.Alias,
                SortOrder = propertyGroup.SortOrder
            };

            if (propertyGroup.HasIdentity)
                dto.Id = propertyGroup.Id;

            dto.PropertyTypeDtos = propertyGroup.PropertyTypes.Select(propertyType => BuildPropertyTypeDto(propertyGroup.Id, propertyType, contentTypeId)).ToList();

            return dto;
        }

        internal static PropertyTypeDto BuildPropertyTypeDto(int groupId, PropertyType propertyType, int contentTypeId)
        {
            var propertyTypeDto = new PropertyTypeDto
            {
                Alias = propertyType.Alias,
                ContentTypeId = contentTypeId,
                DataTypeId = propertyType.DataTypeId,
                Description = propertyType.Description,
                Mandatory = propertyType.Mandatory,
                MandatoryMessage = propertyType.MandatoryMessage,
                Name = propertyType.Name,
                SortOrder = propertyType.SortOrder,
                ValidationRegExp = propertyType.ValidationRegExp,
                ValidationRegExpMessage = propertyType.ValidationRegExpMessage,
                UniqueId = propertyType.Key,
                Variations = (byte)propertyType.Variations,
                LabelOnTop = propertyType.LabelOnTop
            };

            if (groupId != default)
            {
                propertyTypeDto.PropertyTypeGroupId = groupId;
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
