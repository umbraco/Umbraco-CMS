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

        public override void Map(Property property, ContentPropertyDto dest, MapperContext context)
        {
            base.Map(property, dest, context);

            dest.IsRequired = property.PropertyType.Mandatory;
            dest.ValidationRegExp = property.PropertyType.ValidationRegExp;
            dest.Description = property.PropertyType.Description;
            dest.Label = property.PropertyType.Name;
            dest.DataType = DataTypeService.GetDataType(property.PropertyType.DataTypeId);
        }
    }
}
