using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class PropertyFactory
    {
        public static IEnumerable<Property> BuildEntities(IReadOnlyCollection<PropertyDataDto> dtos, PropertyType[] propertyTypes)
        {
            var properties = new List<Property>();

            foreach (var propertyType in propertyTypes)
            {
                var property = propertyType.CreateProperty();

                try
                {
                    property.DisableChangeTracking();

                    var propDtos = dtos.Where(x => x.PropertyTypeId == propertyType.Id);
                    foreach (var propDto in propDtos)
                        property.SetValue(propDto.LanguageId, propDto.Segment, propDto.Value);

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

        private static PropertyDataDto BuildDto(int nodeId, Guid versionId, Property property, Property.PropertyValue propertyValue, bool published)
        {
            var dto = new PropertyDataDto { NodeId = nodeId, VersionId = versionId, PropertyTypeId = property.PropertyTypeId };

            if (property.HasIdentity)
                dto.Id = property.Id;

            if (propertyValue.LanguageId.HasValue)
                dto.LanguageId = propertyValue.LanguageId;

            if (propertyValue.Segment != null)
                dto.Segment = propertyValue.Segment;

            dto.Published = published;

            var value = published ? propertyValue.PublishedValue : propertyValue.DraftValue;

            if (property.DataTypeDatabaseType == DataTypeDatabaseType.Integer)
            {
                if (value is bool || property.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.TrueFalseAlias)
                {
                    dto.IntegerValue = value != null && string.IsNullOrEmpty(value.ToString()) ? 0 : Convert.ToInt32(value);
                }
                else if (value != null && string.IsNullOrWhiteSpace(value.ToString()) == false && int.TryParse(value.ToString(), out var val))
                {
                    dto.IntegerValue = val;
                }
            }
            else if (property.DataTypeDatabaseType == DataTypeDatabaseType.Decimal && value != null)
            {
                if (decimal.TryParse(value.ToString(), out var val))
                {
                    dto.DecimalValue = val; // property value should be normalized already
                }
            }
            else if (property.DataTypeDatabaseType == DataTypeDatabaseType.Date && value != null && string.IsNullOrWhiteSpace(value.ToString()) == false)
            {
                if (DateTime.TryParse(value.ToString(), out var date))
                {
                    dto.DateValue = date;
                }
            }
            else if (property.DataTypeDatabaseType == DataTypeDatabaseType.Ntext && value != null)
            {
                dto.TextValue = value.ToString();
            }
            else if (property.DataTypeDatabaseType == DataTypeDatabaseType.Nvarchar && value != null)
            {
                dto.VarcharValue = value.ToString();
            }

            return dto;
        }

        public static IEnumerable<PropertyDataDto> BuildDtos(int nodeId, Guid versionId, IEnumerable<Property> properties)
        {
            var propertyDataDtos = new List<PropertyDataDto>();

            foreach (var property in properties)
            {
                foreach (var propertyValue in property.Values)
                {
                    if (propertyValue.DraftValue != null)
                        propertyDataDtos.Add(BuildDto(nodeId, versionId, property, propertyValue, false));
                    if (propertyValue.PublishedValue != null)
                        propertyDataDtos.Add(BuildDto(nodeId, versionId, property, propertyValue, true));
                }
            }

            return propertyDataDtos;
        }
    }
}
