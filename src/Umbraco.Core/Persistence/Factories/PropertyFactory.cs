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
                properties.Add(propertyType.CreatePropertyFromRawValue(dto.GetValue));
            }
            return properties;
        }

        public IEnumerable<PropertyDataDto> BuildDto(IEnumerable<Property> properties)
        {
            var propertyDataDtos = new List<PropertyDataDto>();
            /*var serviceStackSerializer = new ServiceStackXmlSerializer();
            var service = new SerializationService(serviceStackSerializer);*/

            foreach (var property in properties)
            {
                var dto = new PropertyDataDto { NodeId = _id, PropertyTypeId = property.PropertyTypeId, VersionId = _version };
                //TODO Add complex (PropertyEditor) ValueModels to the Ntext/Nvarchar column as a serialized 'Object' (DataTypeDatabaseType.Object)
                /*if (property.Value is IEditorModel)
                {
                    var result = service.ToStream(property.Value);
                    dto.Text = result.ResultStream.ToJsonString();
                }*/
                if (property.DataTypeDatabaseType == DataTypeDatabaseType.Integer)
                {
                    dto.Integer = int.Parse(property.Value.ToString());
                }
                else if (property.DataTypeDatabaseType == DataTypeDatabaseType.Date)
                {
                    dto.Date = DateTime.Parse(property.Value.ToString());
                }
                else if (property.DataTypeDatabaseType == DataTypeDatabaseType.Ntext)
                {
                    dto.Text = property.Value.ToString();
                }
                else if (property.DataTypeDatabaseType == DataTypeDatabaseType.Nvarchar)
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
                properties.Add(propertyType.CreatePropertyFromRawValue(dto.GetValue));
            }
            return properties;
        }
    }
}