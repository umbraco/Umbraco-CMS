using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates a ContentPropertyDto from a Property
    /// </summary>
    internal class ContentPropertyDtoMapper : ContentPropertyBasicMapper<ContentPropertyDto>
    {
        public ContentPropertyDtoMapper(IDataTypeService dataTypeService, ILogger logger, PropertyEditorCollection propertyEditors)
            : base(dataTypeService, logger, propertyEditors)
        { }

        public override ContentPropertyDto Map(Property property, ContentPropertyDto dest, MapperContext context)
        {
            var propertyDto = base.Map(property, dest, context);

            propertyDto.IsRequired = property.PropertyType.Mandatory;
            propertyDto.ValidationRegExp = property.PropertyType.ValidationRegExp;
            propertyDto.Description = property.PropertyType.Description;
            propertyDto.Label = property.PropertyType.Name;
            propertyDto.DataType = DataTypeService.GetDataType(property.PropertyType.DataTypeId);

            return propertyDto;
        }
    }
}
