using Microsoft.Extensions.Logging;
using Umbraco.Core.Dictionary;
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
    public class ContentPropertyMapDefinition : IMapDefinition
    {
        private readonly ContentPropertyBasicMapper<ContentPropertyBasic> _contentPropertyBasicConverter;
        private readonly ContentPropertyDtoMapper _contentPropertyDtoConverter;
        private readonly ContentPropertyDisplayMapper _contentPropertyDisplayMapper;

        public ContentPropertyMapDefinition(ICultureDictionary cultureDictionary, IDataTypeService dataTypeService, IEntityService entityService, ILocalizedTextService textService, ILoggerFactory loggerFactory, PropertyEditorCollection propertyEditors)
        {
            _contentPropertyBasicConverter = new ContentPropertyBasicMapper<ContentPropertyBasic>(dataTypeService, entityService, loggerFactory.CreateLogger<ContentPropertyBasicMapper<ContentPropertyBasic>>(), propertyEditors);
            _contentPropertyDtoConverter = new ContentPropertyDtoMapper(dataTypeService, entityService, loggerFactory.CreateLogger<ContentPropertyDtoMapper>(), propertyEditors);
            _contentPropertyDisplayMapper = new ContentPropertyDisplayMapper(cultureDictionary, dataTypeService, entityService, textService, loggerFactory.CreateLogger<ContentPropertyDisplayMapper>(), propertyEditors);
        }

        public void DefineMaps(UmbracoMapper mapper)
        {
            mapper.Define<PropertyGroup, Tab<ContentPropertyDisplay>>((source, context) => new Tab<ContentPropertyDisplay>(), Map);
            mapper.Define<IProperty, ContentPropertyBasic>((source, context) => new ContentPropertyBasic(), Map);
            mapper.Define<IProperty, ContentPropertyDto>((source, context) => new ContentPropertyDto(), Map);
            mapper.Define<IProperty, ContentPropertyDisplay>((source, context) => new ContentPropertyDisplay(), Map);
        }

        // Umbraco.Code.MapAll -Properties -Alias -Expanded
        private void Map(PropertyGroup source, Tab<ContentPropertyDisplay> target, MapperContext mapper)
        {
            target.Id = source.Id;
            target.IsActive = true;
            target.Label = source.Name;
        }

        private void Map(IProperty source, ContentPropertyBasic target, MapperContext context)
        {
            // assume this is mapping everything and no MapAll is required
            _contentPropertyBasicConverter.Map(source, target, context);
        }

        private void Map(IProperty source, ContentPropertyDto target, MapperContext context)
        {
            // assume this is mapping everything and no MapAll is required
            _contentPropertyDtoConverter.Map(source, target, context);
        }

        private void Map(IProperty source, ContentPropertyDisplay target, MapperContext context)
        {
            // assume this is mapping everything and no MapAll is required
            _contentPropertyDisplayMapper.Map(source, target, context);
        }
    }
}
