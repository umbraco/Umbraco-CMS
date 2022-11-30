using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Models.Mapping;

/// <summary>
///     Creates a ContentPropertyDto from a Property
/// </summary>
internal class ContentPropertyDtoMapper : ContentPropertyBasicMapper<ContentPropertyDto>
{
    public ContentPropertyDtoMapper(IDataTypeService dataTypeService, IEntityService entityService, ILogger<ContentPropertyDtoMapper> logger, PropertyEditorCollection propertyEditors)
        : base(dataTypeService, entityService, logger, propertyEditors)
    {
    }

    public override void Map(IProperty property, ContentPropertyDto dest, MapperContext context)
    {
        base.Map(property, dest, context);

        dest.IsRequired = property.PropertyType.Mandatory;
        dest.IsRequiredMessage = property.PropertyType.MandatoryMessage;
        dest.ValidationRegExp = property.PropertyType.ValidationRegExp;
        dest.ValidationRegExpMessage = property.PropertyType.ValidationRegExpMessage;
        dest.Description = property.PropertyType.Description;
        dest.Label = property.PropertyType.Name;
        dest.DataType = DataTypeService.GetDataType(property.PropertyType.DataTypeId);
        dest.LabelOnTop = property.PropertyType.LabelOnTop;
    }
}
