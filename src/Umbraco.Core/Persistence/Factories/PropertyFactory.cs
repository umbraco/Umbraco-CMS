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
            var propsDtos = dtos.GroupBy(x => x.PropertyTypeId).ToDictionary(x => x.Key, x => (IEnumerable<PropertyDataDto>) x);

            foreach (var propertyType in propertyTypes)
            {
                var property = propertyType.CreateProperty();

                try
                {
                    property.DisableChangeTracking();

                    // see notes in BuildDtos - we always have edit+published dtos

                    if (propsDtos.TryGetValue(propertyType.Id, out var propDtos))
                    {
                        foreach (var propDto in propDtos)
                            property.FactorySetValue(propDto.LanguageId, propDto.Segment, propDto.Published, propDto.Value);
                    }

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

        private static PropertyDataDto BuildDto(int nodeId, Guid versionId, Property property, Property.PropertyValue propertyValue, bool published, object value)
        {
            var dto = new PropertyDataDto { NodeId = nodeId, VersionId = versionId, PropertyTypeId = property.PropertyTypeId };

            if (propertyValue.LanguageId.HasValue)
                dto.LanguageId = propertyValue.LanguageId;

            if (propertyValue.Segment != null)
                dto.Segment = propertyValue.Segment;

            dto.Published = published;

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

        public static IEnumerable<PropertyDataDto> BuildDtos(int nodeId, Guid versionId, IEnumerable<Property> properties, out bool edited)
        {
            var propertyDataDtos = new List<PropertyDataDto>();
            edited = false;

            foreach (var property in properties)
            {
                if (property.PropertyType.IsPublishing)
                {
                    // publishing = deal with edit and published values
                    foreach (var propertyValue in property.Values)
                    {
                        // create a dto for both edit and published - make sure we have one for edit
                        // we *could* think of optimizing by creating a dto for edit only if EditValue != PublishedValue
                        // but then queries in db would be way more complicated and require coalescing between edit and
                        // published dtos - not worth it
                        if (propertyValue.PublishedValue != null)
                            propertyDataDtos.Add(BuildDto(nodeId, versionId, property, propertyValue, true, propertyValue.PublishedValue));
                        if (propertyValue.EditValue != null)
                            propertyDataDtos.Add(BuildDto(nodeId, versionId, property, propertyValue, false, propertyValue.EditValue));
                        else if (propertyValue.PublishedValue != null)
                            propertyDataDtos.Add(BuildDto(nodeId, versionId, property, propertyValue, false, propertyValue.PublishedValue));

                        // use explicit equals here, else object comparison fails at comparing eg strings
                        var sameValues = propertyValue.PublishedValue == null ? propertyValue.EditValue == null : propertyValue.PublishedValue.Equals(propertyValue.EditValue);
                        edited |= !sameValues;
                    }
                }
                else
                {
                    foreach (var propertyValue in property.Values)
                    {
                        // not publishing = only deal with edit values
                        if (propertyValue.EditValue != null)
                            propertyDataDtos.Add(BuildDto(nodeId, versionId, property, propertyValue, false, propertyValue.EditValue));
                    }
                }
            }

            return propertyDataDtos;
        }
    }
}
