using System;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates a ContentPropertyDto from a Property
    /// </summary>
    internal class ContentPropertyDtoConverter : ContentPropertyBasicConverter<ContentPropertyDto>
    {
        public ContentPropertyDtoConverter(IDataTypeService dataTypeService)
            : base(dataTypeService)
        {
        }

        public override ContentPropertyDto Convert(ResolutionContext context)
        {
            var propertyDto = base.Convert(context);

            var originalProperty = context.SourceValue as Property;
            if (originalProperty == null)
                throw new InvalidOperationException("Source value is not a property.");

            var dataTypeService = DataTypeService;

            propertyDto.IsRequired = originalProperty.PropertyType.Mandatory;
            propertyDto.ValidationRegExp = originalProperty.PropertyType.ValidationRegExp;
            propertyDto.Description = originalProperty.PropertyType.Description;
            propertyDto.Label = originalProperty.PropertyType.Name;

            //TODO: We should be able to look both of these up at the same time!
            propertyDto.DataType = dataTypeService.GetDataTypeDefinitionById(originalProperty.PropertyType.DataTypeDefinitionId);
            propertyDto.PreValues = dataTypeService.GetPreValuesCollectionByDataTypeId(originalProperty.PropertyType.DataTypeDefinitionId);

            return propertyDto;
        }
    }
}
