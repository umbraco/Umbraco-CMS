using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class PropertyGroupFactory 
    {
        private readonly int _id;
        private readonly DateTime _createDate;
        private readonly DateTime _updateDate;
        //a callback to create a property type which can be injected via a contructor
        private readonly Func<string, DataTypeDatabaseType, string, PropertyType> _propertyTypeCtor;

        public PropertyGroupFactory(int id)
        {
            _id = id;
            _propertyTypeCtor = (propertyEditorAlias, dbType, alias) => new PropertyType(propertyEditorAlias, dbType);
        }

        public PropertyGroupFactory(int id, DateTime createDate, DateTime updateDate, Func<string, DataTypeDatabaseType, string, PropertyType> propertyTypeCtor)
        {
            _id = id;
            _createDate = createDate;
            _updateDate = updateDate;
            _propertyTypeCtor = propertyTypeCtor;
        }

        #region Implementation of IEntityFactory<IEnumerable<PropertyGroup>,IEnumerable<TabDto>>

        public IEnumerable<PropertyGroup> BuildEntity(IEnumerable<PropertyTypeGroupDto> dto)
        {
            var propertyGroups = new PropertyGroupCollection();
            foreach (var groupDto in dto)
            {
                var group = new PropertyGroup();
                //Only assign an Id if the PropertyGroup belongs to this ContentType
                if (groupDto.ContentTypeNodeId == _id)
                {
                    group.Id = groupDto.Id;

                    if (groupDto.ParentGroupId.HasValue)
                        group.ParentId = groupDto.ParentGroupId.Value;
                }
                else
                {
                    //If the PropertyGroup is inherited, we add a reference to the group as a Parent.
                    group.ParentId = groupDto.Id;
                }

                group.Name = groupDto.Text;
                group.SortOrder = groupDto.SortOrder;
                group.PropertyTypes = new PropertyTypeCollection();

                //Because we are likely to have a group with no PropertyTypes we need to ensure that these are excluded
                var typeDtos = groupDto.PropertyTypeDtos.Where(x => x.Id > 0);
                foreach (var typeDto in typeDtos)
                {
                    var tempGroupDto = groupDto;
                    var propertyType = _propertyTypeCtor(typeDto.DataTypeDto.PropertyEditorAlias,
                        typeDto.DataTypeDto.DbType.EnumParse<DataTypeDatabaseType>(true),
                        typeDto.Alias);

                    propertyType.Alias = typeDto.Alias;
                    propertyType.DataTypeDefinitionId = typeDto.DataTypeId;
                    propertyType.Description = typeDto.Description;
                    propertyType.Id = typeDto.Id;
                    propertyType.Name = typeDto.Name;
                    propertyType.Mandatory = typeDto.Mandatory;
                    propertyType.SortOrder = typeDto.SortOrder;
                    propertyType.ValidationRegExp = typeDto.ValidationRegExp;
                    propertyType.PropertyGroupId = new Lazy<int>(() => tempGroupDto.Id);
                    propertyType.CreateDate = _createDate;
                    propertyType.UpdateDate = _updateDate;

                    //on initial construction we don't want to have dirty properties tracked
                    // http://issues.umbraco.org/issue/U4-1946
                    propertyType.ResetDirtyProperties(false);
                    group.PropertyTypes.Add(propertyType);
                }
                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                group.ResetDirtyProperties(false);
                propertyGroups.Add(group);
            }

            return propertyGroups;
        }

        public IEnumerable<PropertyTypeGroupDto> BuildDto(IEnumerable<PropertyGroup> entity)
        {
            return entity.Select(BuildGroupDto).ToList();
        }

        #endregion

        internal PropertyTypeGroupDto BuildGroupDto(PropertyGroup propertyGroup)
        {
            var dto = new PropertyTypeGroupDto
                             {
                                 ContentTypeNodeId = _id,
                                 SortOrder = propertyGroup.SortOrder,
                                 Text = propertyGroup.Name
                             };

            if (propertyGroup.ParentId.HasValue)
                dto.ParentGroupId = propertyGroup.ParentId.Value;

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
                                          Mandatory = propertyType.Mandatory,
                                          Name = propertyType.Name,
                                          SortOrder = propertyType.SortOrder,
                                          ValidationRegExp = propertyType.ValidationRegExp
                                      };

            if (tabId != default(int))
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