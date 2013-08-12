using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates a ContentPropertyDto from a Property
    /// </summary>
    internal class ContentPropertyDtoConverter : ContentPropertyBasicConverter<ContentPropertyDto>
    {
        private readonly ApplicationContext _applicationContext;

        public ContentPropertyDtoConverter(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        protected override ContentPropertyDto ConvertCore(Property originalProperty)
        {
            var propertyDto = base.ConvertCore(originalProperty);

            propertyDto.IsRequired = originalProperty.PropertyType.Mandatory;
            propertyDto.ValidationRegExp = originalProperty.PropertyType.ValidationRegExp;
            propertyDto.Alias = originalProperty.Alias;
            propertyDto.Description = originalProperty.PropertyType.Description;
            propertyDto.Label = originalProperty.PropertyType.Name;
            propertyDto.DataType = _applicationContext.Services.DataTypeService.GetDataTypeDefinitionById(originalProperty.PropertyType.DataTypeDefinitionId);
            propertyDto.PropertyEditor = PropertyEditorResolver.Current.GetById(originalProperty.PropertyType.DataTypeId);

            return propertyDto;
        }
    }
}