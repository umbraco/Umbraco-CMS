using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// A mapper which declares how to map content properties. These mappings are shared among media (and probably members) which is
    /// why they are in their own mapper
    /// </summary>
    internal class ContentPropertyMapperProfile : IMapperProfile
    {
        private readonly ContentPropertyBasicMapper<ContentPropertyBasic> _contentPropertyBasicConverter;
        private readonly ContentPropertyDtoMapper _contentPropertyDtoConverter;
        private readonly ContentPropertyDisplayMapper _contentPropertyDisplayMapper;

        public ContentPropertyMapperProfile(IDataTypeService dataTypeService, ILocalizedTextService textService, ILogger logger, PropertyEditorCollection propertyEditors)
        {
            _contentPropertyBasicConverter = new ContentPropertyBasicMapper<ContentPropertyBasic>(dataTypeService, logger, propertyEditors);
            _contentPropertyDtoConverter = new ContentPropertyDtoMapper(dataTypeService, logger, propertyEditors);
            _contentPropertyDisplayMapper = new ContentPropertyDisplayMapper(dataTypeService, textService, logger, propertyEditors);
        }

        public void SetMaps(Mapper mapper)
        {
            mapper.Define<PropertyGroup, Tab<ContentPropertyDisplay>>((source, context) => new Tab<ContentPropertyDisplay>(), Map);
            mapper.Define<Property, ContentPropertyBasic>((source, context) => new ContentPropertyBasic(), Map);
            mapper.Define<Property, ContentPropertyDto>((source, context) => new ContentPropertyDto(), Map);
            mapper.Define<Property, ContentPropertyDisplay>((source, context) => new ContentPropertyDisplay(), Map);
        }

        // Umbraco.Code.MapAll -Properties -Alias -Expanded
        private void Map(PropertyGroup source, Tab<ContentPropertyDisplay> target, MapperContext mapper)
        {
            target.Id = source.Id;
            target.IsActive = true;
            target.Label = source.Name;
}

        private void Map(Property source, ContentPropertyBasic target, MapperContext context)
        {
            // assume this is mapping everything and no MapAll is required
            _contentPropertyBasicConverter.Map(source, target, context);
        }

        private void Map(Property source, ContentPropertyDto target, MapperContext context)
        {
            // assume this is mapping everything and no MapAll is required
            _contentPropertyDtoConverter.Map(source, target, context);
        }

        private void Map(Property source, ContentPropertyDisplay target, MapperContext context)
        {
            // assume this is mapping everything and no MapAll is required
            _contentPropertyDisplayMapper.Map(source, target, context);
        }
    }
}
