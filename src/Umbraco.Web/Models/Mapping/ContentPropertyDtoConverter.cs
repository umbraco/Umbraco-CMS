using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates a ContentPropertyDto from a Property
    /// </summary>
    internal class ContentPropertyDtoConverter : ContentPropertyBasicConverter<ContentPropertyDto>
    {
        public ContentPropertyDtoConverter(IDataTypeService dataTypeService, ILogger logger, PropertyEditorCollection propertyEditors)
            : base(dataTypeService, logger, propertyEditors)
        { }

        public override ContentPropertyDto Convert(Property property, ContentPropertyDto dest, ResolutionContext context)
        {
            var propertyDto = base.Convert(property, dest, context);

            propertyDto.IsRequired = property.PropertyType.Mandatory;
            propertyDto.ValidationRegExp = property.PropertyType.ValidationRegExp;
            propertyDto.Description = property.PropertyType.Description;
            propertyDto.Label = property.PropertyType.Name;
            propertyDto.DataType = DataTypeService.GetDataType(property.PropertyType.DataTypeId);

            //Get the culture from the context which will be set during the mapping operation for each property
            var culture = context.GetCulture();

            //a culture needs to be in the context for a property type that can vary
            if (culture == null && property.PropertyType.VariesByCulture())
                throw new InvalidOperationException($"No culture found in mapping operation when one is required for the culture variant property type {property.PropertyType.Alias}");

            propertyDto.Culture = culture;

            return propertyDto;
        }
    }
}
