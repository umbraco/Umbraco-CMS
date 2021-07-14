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
    internal class ContentPropertyMapDefinition : IMapDefinition
    {
        private readonly ContentPropertyBasicMapper<ContentPropertyBasic> _contentPropertyBasicConverter;
        private readonly ContentPropertyDtoMapper _contentPropertyDtoConverter;
        private readonly ContentPropertyDisplayMapper _contentPropertyDisplayMapper;

        public ContentPropertyMapDefinition(IDataTypeService dataTypeService, IEntityService entityService, ILocalizedTextService textService, ILogger logger, PropertyEditorCollection propertyEditors)
        {
            _contentPropertyBasicConverter = new ContentPropertyBasicMapper<ContentPropertyBasic>(dataTypeService, entityService, logger, propertyEditors);
            _contentPropertyDtoConverter = new ContentPropertyDtoMapper(dataTypeService, entityService, logger, propertyEditors);
            _contentPropertyDisplayMapper = new ContentPropertyDisplayMapper(dataTypeService, entityService, textService, logger, propertyEditors);
        }

        public void DefineMaps(UmbracoMapper mapper)
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
            target.Key = source.Key;
            target.Type = (int)source.Type;
            target.Label = source.Name;
            target.Alias = source.Alias;
            target.IsActive = true;
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
