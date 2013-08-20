using System;
using Umbraco.Core;
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
        private readonly Lazy<IDataTypeService> _dataTypeService;

        public ContentPropertyDtoConverter(Lazy<IDataTypeService> dataTypeService)
        {
            _dataTypeService = dataTypeService;
        }

        protected override ContentPropertyDto ConvertCore(Property originalProperty)
        {
            var propertyDto = base.ConvertCore(originalProperty);

            var dataTypeService = (DataTypeService)_dataTypeService.Value;

            propertyDto.IsRequired = originalProperty.PropertyType.Mandatory;
            propertyDto.ValidationRegExp = originalProperty.PropertyType.ValidationRegExp;
            propertyDto.Description = originalProperty.PropertyType.Description;
            propertyDto.Label = originalProperty.PropertyType.Name;
            propertyDto.DataType = dataTypeService.GetDataTypeDefinitionById(originalProperty.PropertyType.DataTypeDefinitionId);

            return propertyDto;
        }
    }
}