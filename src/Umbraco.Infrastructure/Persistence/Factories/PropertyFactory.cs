using System.Globalization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class PropertyFactory
{
    public static IEnumerable<IProperty> BuildEntities(IPropertyType[]? propertyTypes, IReadOnlyCollection<PropertyDataDto> dtos, int publishedVersionId, ILanguageRepository languageRepository)
    {
        var properties = new List<IProperty>();
        var xdtos = dtos.GroupBy(x => x.PropertyTypeId).ToDictionary(x => x.Key, x => (IEnumerable<PropertyDataDto>)x);

        if (propertyTypes is null)
        {
            return properties;
        }

        foreach (IPropertyType propertyType in propertyTypes)
        {
            var values = new List<Property.InitialPropertyValue>();
            int propertyId = default;

            // see notes in BuildDtos - we always have edit+published dtos
            if (xdtos.TryGetValue(propertyType.Id, out IEnumerable<PropertyDataDto>? propDtos))
            {
                foreach (PropertyDataDto propDto in propDtos)
                {
                    propertyId = propDto.Id;
                    values.Add(new Property.InitialPropertyValue(
                        languageRepository.GetIsoCodeById(propDto.LanguageId),
                        propDto.Segment,
                        propDto.VersionId == publishedVersionId,
                        propDto.Value));
                }
            }

            var property = Property.CreateWithValues(propertyId, propertyType, values.ToArray());
            properties.Add(property);
        }

        return properties;
    }

    /// <summary>
    ///     Creates a collection of <see cref="PropertyDataDto" /> from a collection of <see cref="Property" />
    /// </summary>
    /// <param name="contentVariation">
    ///     The <see cref="ContentVariation" /> of the entity containing the collection of <see cref="Property" />
    /// </param>
    /// <param name="currentVersionId"></param>
    /// <param name="publishedVersionId"></param>
    /// <param name="properties">The properties to map</param>
    /// <param name="languageRepository"></param>
    /// <param name="edited">out parameter indicating that one or more properties have been edited</param>
    /// <param name="editedCultures">
    ///     Out parameter containing a collection of edited cultures when the contentVariation varies by culture.
    ///     The value of this will be used to populate the edited cultures in the umbracoDocumentCultureVariation table.
    /// </param>
    /// <returns></returns>
    public static IEnumerable<PropertyDataDto> BuildDtos(
        ContentVariation contentVariation,
        int currentVersionId,
        int publishedVersionId,
        IEnumerable<IProperty> properties,
        ILanguageRepository languageRepository,
        out bool edited,
        out HashSet<string>? editedCultures)
    {
        var propertyDataDtos = new List<PropertyDataDto>();
        edited = false;
        editedCultures = null; // don't allocate unless necessary
        string? defaultCulture = null; // don't allocate unless necessary

        var entityVariesByCulture = contentVariation.VariesByCulture();

        // create dtos for each property values, but only for values that do actually exist
        // ie have a non-null value, everything else is just ignored and won't have a db row
        foreach (IProperty property in properties)
        {
            if (property.PropertyType.SupportsPublishing)
            {
                // create the resulting hashset if it's not created and the entity varies by culture
                if (entityVariesByCulture && editedCultures == null)
                {
                    editedCultures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }

                // publishing = deal with edit and published values
                foreach (IPropertyValue propertyValue in property.Values)
                {
                    var isInvariantValue = propertyValue.Culture == null && propertyValue.Segment == null;
                    var isCultureValue = propertyValue.Culture != null;
                    var isSegmentValue = propertyValue.Segment != null;

                    // deal with published value
                    if ((propertyValue.PublishedValue != null || isSegmentValue) && publishedVersionId > 0)
                    {
                        propertyDataDtos.Add(BuildDto(publishedVersionId, property, languageRepository.GetIdByIsoCode(propertyValue.Culture), propertyValue.Segment, propertyValue.PublishedValue));
                    }

                    // deal with edit value
                    if (propertyValue.EditedValue != null || isSegmentValue)
                    {
                        propertyDataDtos.Add(BuildDto(currentVersionId, property, languageRepository.GetIdByIsoCode(propertyValue.Culture), propertyValue.Segment, propertyValue.EditedValue));
                    }

                    // property.Values will contain ALL of it's values, both variant and invariant which will be populated if the
                    // administrator has previously changed the property type to be variant vs invariant.
                    // We need to check for this scenario here because otherwise the editedCultures and edited flags
                    // will end up incorrectly set in the umbracoDocumentCultureVariation table so here we need to
                    // only process edited cultures based on the current value type and how the property varies.
                    // The above logic will still persist the currently saved property value for each culture in case the admin
                    // decides to swap the property's variance again, in which case the edited flag will be recalculated.
                    if ((property.PropertyType.VariesByCulture() && isInvariantValue) ||
                        (!property.PropertyType.VariesByCulture() && isCultureValue))
                    {
                        continue;
                    }

                    // use explicit equals here, else object comparison fails at comparing eg strings
                    var sameValues = propertyValue?.PublishedValue == null
                        ? propertyValue?.EditedValue == null
                        : propertyValue.PublishedValue.Equals(propertyValue.EditedValue);

                    edited |= !sameValues;

                    if (entityVariesByCulture && !sameValues)
                    {
                        if (isCultureValue && propertyValue?.Culture is not null)
                        {
                            editedCultures?.Add(propertyValue.Culture); // report culture as edited
                        }
                        else if (isInvariantValue)
                        {
                            // flag culture as edited if it contains an edited invariant property
                            if (defaultCulture == null)
                            {
                                defaultCulture = languageRepository.GetDefaultIsoCode();
                            }

                            editedCultures?.Add(defaultCulture);
                        }
                    }
                }
            }
            else
            {
                foreach (IPropertyValue propertyValue in property.Values)
                {
                    // not publishing = only deal with edit values
                    if (propertyValue.EditedValue != null)
                    {
                        propertyDataDtos.Add(BuildDto(currentVersionId, property, languageRepository.GetIdByIsoCode(propertyValue.Culture), propertyValue.Segment, propertyValue.EditedValue));
                    }
                }

                edited = true;
            }
        }

        return propertyDataDtos;
    }

    private static PropertyDataDto BuildDto(int versionId, IProperty property, int? languageId, string? segment, object? value)
    {
        var dto = new PropertyDataDto { VersionId = versionId, PropertyTypeId = property.PropertyTypeId };

        if (languageId.HasValue)
        {
            dto.LanguageId = languageId;
        }

        if (segment != null)
        {
            dto.Segment = segment;
        }

        if (property.ValueStorageType == ValueStorageType.Integer)
        {
            if (value is bool || property.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.Boolean)
            {
                dto.IntegerValue = value != null && string.IsNullOrEmpty(value.ToString()) ? 0 : Convert.ToInt32(value);
            }
            else if (value != null && string.IsNullOrWhiteSpace(value.ToString()) == false &&
                     int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var val))
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
        else if (property.ValueStorageType == ValueStorageType.Date && value != null &&
                 string.IsNullOrWhiteSpace(value.ToString()) == false)
        {
            if (DateTime.TryParse(value.ToString(), out DateTime date))
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
}
