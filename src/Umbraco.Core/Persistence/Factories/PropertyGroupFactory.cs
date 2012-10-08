using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class PropertyGroupFactory : IEntityFactory<IEnumerable<PropertyGroup>, IEnumerable<TabDto>>
    {
        private int _id;

        public PropertyGroupFactory(int id)
        {
            _id = id;
        }

        #region Implementation of IEntityFactory<IEnumerable<PropertyGroup>,IEnumerable<TabDto>>

        public IEnumerable<PropertyGroup> BuildEntity(IEnumerable<TabDto> dto)
        {
            var propertyGroups = new PropertyGroupCollection();
            foreach (var tabDto in dto)
            {
                var group = new PropertyGroup();
                group.Id = tabDto.Id;
                group.Name = tabDto.Text;
                group.SortOrder = tabDto.SortOrder;
                group.PropertyTypes = new PropertyTypeCollection();

                foreach (var typeDto in tabDto.PropertyTypeDtos)
                {
                    group.PropertyTypes.Add(new PropertyType(typeDto.DataTypeDto.ControlId,
                                                             typeDto.DataTypeDto.DbType.EnumParse<DataTypeDatabaseType>(true))
                                                {
                                                    Alias = typeDto.Alias,
                                                    DataTypeId = typeDto.DataTypeId,
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

        public IEnumerable<TabDto> BuildDto(IEnumerable<PropertyGroup> entity)
        {
            return entity.Select(propertyGroup => BuildTabDto(propertyGroup)).ToList();
        }

        #endregion

        internal TabDto BuildTabDto(PropertyGroup propertyGroup)
        {
            var tabDto = new TabDto
                             {
                                 ContentTypeNodeId = _id,
                                 SortOrder = propertyGroup.SortOrder,
                                 Text = propertyGroup.Name
                             };

            if (propertyGroup.HasIdentity)
                tabDto.Id = propertyGroup.Id;

            tabDto.PropertyTypeDtos = propertyGroup.PropertyTypes.Select(propertyType => BuildPropertyTypeDto(propertyGroup.Id, propertyType)).ToList();

            return tabDto;
        }

        internal PropertyTypeDto BuildPropertyTypeDto(int tabId, PropertyType propertyType)
        {
            var propertyTypeDto = new PropertyTypeDto
                                      {
                                          Alias = propertyType.Alias,
                                          ContentTypeId = _id,
                                          DataTypeId = propertyType.DataTypeId,
                                          Description = propertyType.Description,
                                          HelpText = propertyType.HelpText,
                                          Mandatory = propertyType.Mandatory,
                                          Name = propertyType.Name,
                                          SortOrder = propertyType.SortOrder,
                                          TabId = tabId
                                      };

            if (propertyType.HasIdentity)
                propertyTypeDto.Id = propertyType.Id;

            return propertyTypeDto;
        }
    }
}