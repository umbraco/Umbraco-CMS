using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class PropertyFactory : IEntityFactory<IEnumerable<Property>, IEnumerable<PropertyDataDto>>
    {
        private readonly IContentTypeComposition _contentType;
        private readonly Guid _version;
        private readonly int _id;
        private readonly DateTime _createDate;
        private readonly DateTime _updateDate;

        public PropertyFactory(IContentTypeComposition contentType, Guid version, int id)
        {
            _contentType = contentType;
            _version = version;
            _id = id;
        }

        public PropertyFactory(IContentTypeComposition contentType, Guid version, int id, DateTime createDate, DateTime updateDate)
        {
            _contentType = contentType;
            _version = version;
            _id = id;
            _createDate = createDate;
            _updateDate = updateDate;
        }

        #region Implementation of IEntityFactory<IContent,PropertyDataDto>

        public IEnumerable<Property> BuildEntity(IEnumerable<PropertyDataDto> dtos)
        {
            var properties = new List<Property>();

            foreach (var propertyType in _contentType.CompositionPropertyTypes)
            {
                var propertyDataDto = dtos.LastOrDefault(x => x.PropertyTypeId == propertyType.Id);
                var property = propertyDataDto == null
                                   ? propertyType.CreatePropertyFromValue(null)
                                   : propertyType.CreatePropertyFromRawValue(propertyDataDto.GetValue,
                                                                             propertyDataDto.VersionId.Value,
                                                                             propertyDataDto.Id);

                property.CreateDate = _createDate;
                property.UpdateDate = _updateDate;
                property.ResetDirtyProperties();
                properties.Add(property);
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
                    dto.Id = property.Id;

                if (property.DataTypeDatabaseType == DataTypeDatabaseType.Integer)
                {
                    if (property.Value is bool || property.PropertyType.DataTypeId == new Guid("38b352c1-e9f8-4fd8-9324-9a2eab06d97a"))
                    {
                        int val = Convert.ToInt32(property.Value);
                        dto.Integer = val;
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
                else if (property.DataTypeDatabaseType == DataTypeDatabaseType.Date && property.Value != null && string.IsNullOrWhiteSpace(property.Value.ToString()) == false)
                {
                    DateTime date;
                    if(DateTime.TryParse(property.Value.ToString(), out date))
                        dto.Date = date;
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

        #endregion
    }
}