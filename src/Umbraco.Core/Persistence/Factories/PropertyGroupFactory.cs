using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class PropertyGroupFactory : IEntityFactory<IEnumerable<PropertyGroup>, IEnumerable<PropertyTypeGroupDto>>
    {
        private readonly int _id;

        public PropertyGroupFactory(int id)
        {
            _id = id;
        }

        #region Implementation of IEntityFactory<IEnumerable<PropertyGroup>,IEnumerable<TabDto>>

        public IEnumerable<PropertyGroup> BuildEntity(IEnumerable<PropertyTypeGroupDto> dto)
        {
            var propertyGroups = new PropertyGroupCollection();
            foreach (var groupDto in dto)
            {
                var group = new PropertyGroup();
                group.Id = groupDto.Id;
                group.Name = groupDto.Text;
                group.ParentId = groupDto.ParentGroupId;
                group.SortOrder = groupDto.SortOrder;
                group.PropertyTypes = new PropertyTypeCollection();

                foreach (var typeDto in groupDto.PropertyTypeDtos)
                {
                    group.PropertyTypes.Add(new PropertyType(typeDto.DataTypeDto.ControlId,
                                                             typeDto.DataTypeDto.DbType.EnumParse<DataTypeDatabaseType>(true))
                                                {
                                                    Alias = typeDto.Alias,
                                                    DataTypeDefinitionId = typeDto.DataTypeId,
                                                    Description = typeDto.Description,
                                                    Id = typeDto.Id,
                                                    Name = typeDto.Name,
                                                    HelpText = typeDto.HelpText,
                                                    Mandatory = typeDto.Mandatory,
                                                    SortOrder = typeDto.SortOrder
                                                });

                }
                group.ResetDirtyProperties();
                propertyGroups.Add(group);
            }

            return propertyGroups;
        }

        public IEnumerable<PropertyTypeGroupDto> BuildDto(IEnumerable<PropertyGroup> entity)
        {
            return entity.Select(propertyGroup => BuildGroupDto(propertyGroup)).ToList();
        }

        #endregion

        internal PropertyTypeGroupDto BuildGroupDto(PropertyGroup propertyGroup)
        {
            var dto = new PropertyTypeGroupDto
                             {
                                 ContentTypeNodeId = _id,
                                 SortOrder = propertyGroup.SortOrder,
                                 Text = propertyGroup.Name,
                                 ParentGroupId = propertyGroup.ParentId
                             };

            if (propertyGroup.HasIdentity)
                dto.Id = propertyGroup.Id;

            dto.PropertyTypeDtos = propertyGroup.PropertyTypes.Select(propertyType => BuildPropertyTypeDto(propertyGroup.Id, propertyType)).ToList();

            return dto;
        }

        internal PropertyTypeDto BuildPropertyTypeDto(int tabId, PropertyType propertyType)
        {
            var propertyTypeDto = new PropertyTypeDto
                                      {
                                          Alias = propertyType.Alias,
                                          ContentTypeId = _id,
                                          DataTypeId = propertyType.DataTypeDefinitionId,
                                          Description = propertyType.Description,
                                          HelpText = propertyType.HelpText,
                                          Mandatory = propertyType.Mandatory,
                                          Name = propertyType.Name,
                                          SortOrder = propertyType.SortOrder,
                                          PropertyTypeGroupId = tabId
                                      };

            if (propertyType.HasIdentity)
                propertyTypeDto.Id = propertyType.Id;

            return propertyTypeDto;
        }
    }
}