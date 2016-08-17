using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class PropertyFactory
    {
        private readonly PropertyType[] _compositionTypeProperties;
        private readonly Guid _version;
        private readonly int _id;
        private readonly DateTime _createDate;
        private readonly DateTime _updateDate;

        public PropertyFactory(PropertyType[] compositionTypeProperties, Guid version, int id)
        {
            _compositionTypeProperties = compositionTypeProperties;
            _version = version;
            _id = id;
        }

        public PropertyFactory(PropertyType[] compositionTypeProperties, Guid version, int id, DateTime createDate, DateTime updateDate)
        {
            _compositionTypeProperties = compositionTypeProperties;
            _version = version;
            _id = id;
            _createDate = createDate;
            _updateDate = updateDate;
        }

        public IEnumerable<Property> BuildEntity(PropertyDataDto[] dtos)
        {
            var properties = new List<Property>();

            foreach (var propertyType in _compositionTypeProperties)
            {
                var propertyDataDto = dtos.LastOrDefault(x => x.PropertyTypeId == propertyType.Id);
                var property = propertyDataDto == null
                                   ? propertyType.CreatePropertyFromValue(null)
                                   : propertyType.CreatePropertyFromRawValue(propertyDataDto.GetValue,
                                                                             propertyDataDto.VersionId.Value,
                                                                             propertyDataDto.Id);
                try
                {
                    //on initial construction we don't want to have dirty properties tracked
                    property.DisableChangeTracking();

                    property.CreateDate = _createDate;
                    property.UpdateDate = _updateDate;
                    // http://issues.umbraco.org/issue/U4-1946
                    property.ResetDirtyProperties(false);
                    properties.Add(property);
                }
                finally
                {
                    property.EnableChangeTracking();
                }
                
            }

            return properties;
        }

        public IEnumerable<PropertyDataDto> BuildDto(IEnumerable<Property> properties)
        {
            var propertyDataDtos = new List<PropertyDataDto>();

            foreach (var property in properties)
            {
                var dto = new PropertyDataDto { NodeId = _id, PropertyTypeId = property.PropertyTypeId, VersionId = _version };

                //Check if property has an Id and set it, so that it can be updated if it already exists
                if (property.HasIdentity)
                {
                    dto.Id = property.Id;
                }

                if (property.DataTypeDatabaseType == DataTypeDatabaseType.Integer)
                {
                    if (property.Value is bool || property.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.TrueFalseAlias)
                    {
                        dto.Integer = property.Value != null && string.IsNullOrEmpty(property.Value.ToString())
                                          ? 0
                                          : Convert.ToInt32(property.Value);
                    }
                    else
                    {
                        int val;
                        if ((property.Value != null && string.IsNullOrWhiteSpace(property.Value.ToString()) == false) && int.TryParse(property.Value.ToString(), out val))
                        {
                            dto.Integer = val;
                        }
                    }
                }
                else if (property.DataTypeDatabaseType == DataTypeDatabaseType.Decimal && property.Value != null)
                {
                    decimal val;
                    if (decimal.TryParse(property.Value.ToString(), out val))
                    {
                        dto.Decimal = val; // property value should be normalized already
                    }
                }
                else if (property.DataTypeDatabaseType == DataTypeDatabaseType.Date && property.Value != null && string.IsNullOrWhiteSpace(property.Value.ToString()) == false)
                {
                    DateTime date;
                    if (DateTime.TryParse(property.Value.ToString(), out date))
                    {
                        dto.Date = date;
                    }
                }
                else if (property.DataTypeDatabaseType == DataTypeDatabaseType.Ntext && property.Value != null)
                {
                    dto.Text = property.Value.ToString();
                }
                else if (property.DataTypeDatabaseType == DataTypeDatabaseType.Nvarchar && property.Value != null)
                {
                    dto.VarChar = property.Value.ToString();
                }

                propertyDataDtos.Add(dto);
            }
            return propertyDataDtos;
        }

    }
}