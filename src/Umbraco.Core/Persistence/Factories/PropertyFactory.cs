using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class PropertyFactory
    {
        public static IEnumerable<Property> BuildEntities(PropertyType[] propertyTypes, IReadOnlyCollection<PropertyDataDto> dtos, int publishedVersionId, ILanguageRepository languageRepository)
        {
            var properties = new List<Property>();
            var xdtos = dtos.GroupBy(x => x.PropertyTypeId).ToDictionary(x => x.Key, x => (IEnumerable<PropertyDataDto>) x);

            foreach (var propertyType in propertyTypes)
            {
                var property = propertyType.CreateProperty();

                try
                {
                    property.DisableChangeTracking();

                    // see notes in BuildDtos - we always have edit+published dtos

                    if (xdtos.TryGetValue(propertyType.Id, out var propDtos))
                    {
                        foreach (var propDto in propDtos)
                        {
                            property.Id = propDto.Id;
                            property.FactorySetValue(languageRepository.GetIsoCodeById(propDto.LanguageId), propDto.Segment, propDto.VersionId == publishedVersionId, propDto.Value);
                        }

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

        private static PropertyDataDto BuildDto(int versionId, Property property, int? languageId, string segment, object value)
        {
            var dto = new PropertyDataDto { VersionId = versionId, PropertyTypeId = property.PropertyTypeId };

            if (languageId.HasValue)
                dto.LanguageId = languageId;

            if (segment != null)
                dto.Segment = segment;

            if (property.ValueStorageType == ValueStorageType.Integer)
            {
                if (value is bool || property.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.Boolean)
                {
                    dto.IntegerValue = value != null && string.IsNullOrEmpty(value.ToString()) ? 0 : Convert.ToInt32(value);
                }
                else if (value != null && string.IsNullOrWhiteSpace(value.ToString()) == false && int.TryParse(value.ToString(), out var val))
                {
                    dto.IntegerValue = val;
                }
            }
            else if (property.ValueStorageType == ValueStorageType.Decimal && value != null)
            {
                if (decimal.TryParse(value.ToString(), out var val))
                {
                    dto.DecimalValue = val; // property value should be normalized already
                }
            }
            else if (property.ValueStorageType == ValueStorageType.Date && value != null && string.IsNullOrWhiteSpace(value.ToString()) == false)
            {
                if (DateTime.TryParse(value.ToString(), out var date))
                {
                    dto.DateValue = date;
                }
            }
            else if (property.ValueStorageType == ValueStorageType.Ntext && value != null)
            {
                dto.TextValue = value.ToString();
            }
            else if (property.ValueStorageType == ValueStorageType.Nvarchar && value != null)
            {
                dto.VarcharValue = value.ToString();
            }

            return dto;
        }

        /// <summary>
        /// Creates a collection of <see cref="PropertyDataDto"/> from a collection of <see cref="Property"/>
        /// </summary>
        /// <param name="contentVariation">
        /// The <see cref="ContentVariation"/> of the entity containing the collection of <see cref="Property"/>
        /// </param>
        /// <param name="currentVersionId"></param>
        /// <param name="publishedVersionId"></param>
        /// <param name="properties">The properties to map</param>
        /// <param name="languageRepository"></param>
        /// <param name="edited">out parameter indicating that one or more properties have been edited</param>
        /// <param name="editedCultures">out parameter containing a collection of edited cultures when the contentVariation varies by culture</param>
        /// <returns></returns>
        public static IEnumerable<PropertyDataDto> BuildDtos(ContentVariation contentVariation, int currentVersionId, int publishedVersionId, IEnumerable<Property> properties,
            ILanguageRepository languageRepository, out bool edited, out HashSet<string> editedCultures)
        {
            var propertyDataDtos = new List<PropertyDataDto>();
            edited = false;
            editedCultures = null; // don't allocate unless necessary
            string defaultCulture = null; //don't allocate unless necessary

            var entityVariesByCulture = contentVariation.VariesByCulture();

            // create dtos for each property values, but only for values that do actually exist
            // ie have a non-null value, everything else is just ignored and won't have a db row

            foreach (var property in properties)
            {
                if (property.PropertyType.IsPublishing)
                {
                    //create the resulting hashset if it's not created and the entity varies by culture
                    if (entityVariesByCulture && editedCultures == null)
                        editedCultures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    // publishing = deal with edit and published values
                    foreach (var propertyValue in property.Values)
                    {
                        // deal with published value
                        if (propertyValue.PublishedValue != null && publishedVersionId > 0)
                            propertyDataDtos.Add(BuildDto(publishedVersionId, property, languageRepository.GetIdByIsoCode(propertyValue.Culture), propertyValue.Segment, propertyValue.PublishedValue));

                        // deal with edit value
                        if (propertyValue.EditedValue != null)
                            propertyDataDtos.Add(BuildDto(currentVersionId, property, languageRepository.GetIdByIsoCode(propertyValue.Culture), propertyValue.Segment, propertyValue.EditedValue));

                        // use explicit equals here, else object comparison fails at comparing eg strings
                        var sameValues = propertyValue.PublishedValue == null ? propertyValue.EditedValue == null : propertyValue.PublishedValue.Equals(propertyValue.EditedValue);
                        edited |= !sameValues;

                        if (entityVariesByCulture // cultures can be edited, ie CultureNeutral is supported
                            && propertyValue.Culture != null && propertyValue.Segment == null // and value is CultureNeutral
                            && !sameValues) // and edited and published are different
                        {
                            editedCultures.Add(propertyValue.Culture); // report culture as edited
                        }

                        // flag culture as edited if it contains an edited invariant property
                        if (propertyValue.Culture == null //invariant property
                            && !sameValues // and edited and published are different
                            && entityVariesByCulture) //only when the entity is variant
                        {
                            if (defaultCulture == null)
                                defaultCulture = languageRepository.GetDefaultIsoCode();

                            editedCultures.Add(defaultCulture);
                        }
                    }
                }
                else
                {
                    foreach (var propertyValue in property.Values)
                    {
                        // not publishing = only deal with edit values
                        if (propertyValue.EditedValue != null)
                            propertyDataDtos.Add(BuildDto(currentVersionId, property,  languageRepository.GetIdByIsoCode(propertyValue.Culture), propertyValue.Segment, propertyValue.EditedValue));
                    }
                    edited = true;
                }
            }

            return propertyDataDtos;
        }
    }
}
