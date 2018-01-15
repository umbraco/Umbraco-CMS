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
        public ContentPropertyDtoConverter(Lazy<IDataTypeService> dataTypeService)
            : base(dataTypeService)
        { }

        public override ContentPropertyDto Convert(Property originalProperty, ContentPropertyDto dest, ResolutionContext context)
        {
            var propertyDto = base.Convert(originalProperty, dest, context);

            var dataTypeService = DataTypeService.Value;

            propertyDto.IsRequired = originalProperty.PropertyType.Mandatory;
            propertyDto.ValidationRegExp = originalProperty.PropertyType.ValidationRegExp;
            propertyDto.Description = originalProperty.PropertyType.Description;
            propertyDto.Label = originalProperty.PropertyType.Name;

            //TODO: We should be able to look both of these up at the same time!
            propertyDto.DataType = dataTypeService.GetDataType(originalProperty.PropertyType.DataTypeDefinitionId);
            propertyDto.PreValues = dataTypeService.GetPreValuesCollectionByDataTypeId(originalProperty.PropertyType.DataTypeDefinitionId);

            return propertyDto;
        }
    }
}
