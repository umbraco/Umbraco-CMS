using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class PropertyFactory : IEntityFactory<IEnumerable<Property>, IEnumerable<PropertyDataDto>>
    {
        private readonly IContentType _contentType;
        private readonly IMediaType _mediaType;
        private readonly Guid _version;
        private readonly int _id;

        public PropertyFactory(IContentType contentType, Guid version, int id)
        {
            _contentType = contentType;
            _version = version;
            _id = id;
        }

        public PropertyFactory(IMediaType mediaType, Guid version, int id)
        {
            _mediaType = mediaType;
            _version = version;
            _id = id;
        }

        #region Implementation of IEntityFactory<IContent,PropertyDataDto>

        public IEnumerable<Property> BuildEntity(IEnumerable<PropertyDataDto> dtos)
        {
            var properties = new List<Property>();
            foreach (var dto in dtos)
            {
                var propertyType = _contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Id == dto.PropertyTypeId);
                var property = propertyType.CreatePropertyFromRawValue(dto.GetValue, dto.VersionId.Value, dto.Id);
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

                if (property.DataTypeDatabaseType == DataTypeDatabaseType.Integer && property.Value != null)
                {
                    dto.Integer = int.Parse(property.Value.ToString());
                }
                else if (property.DataTypeDatabaseType == DataTypeDatabaseType.Date && property.Value != null)
                {
                    dto.Date = DateTime.Parse(property.Value.ToString());
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

        public IEnumerable<Property> BuildMediaEntity(IEnumerable<PropertyDataDto> dtos)
        {
            var properties = new List<Property>();
            foreach (var dto in dtos)
            {
                var propertyType = _mediaType.PropertyTypes.FirstOrDefault(x => x.Id == dto.PropertyTypeId);
                var property = propertyType.CreatePropertyFromRawValue(dto.GetValue, dto.VersionId.Value, dto.Id);
                property.ResetDirtyProperties();
                properties.Add(property);
            }
            return properties;
        }
    }
}