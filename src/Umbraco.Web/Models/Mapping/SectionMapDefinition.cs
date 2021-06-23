using Umbraco.Core.Manifest;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models.Sections;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Sections;

namespace Umbraco.Web.Models.Mapping
{
    internal class SectionMapDefinition : IMapDefinition
    {
        private readonly ILocalizedTextService _textService;
        public SectionMapDefinition(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public void DefineMaps(UmbracoMapper mapper)
        {
            mapper.Define<ISection, Section>((source, context) => new Section(), Map);

            // this is for AutoMapper ReverseMap - but really?
            mapper.Define<Section, ContentSection>();
            mapper.Define<Section, ContentSection>();
            mapper.Define<Section, ManifestSection>(Map);
            mapper.Define<Section, MediaSection>();
            mapper.Define<Section, MembersSection>();
            mapper.Define<Section, PackagesSection>();
            mapper.Define<Section, SettingsSection>();
            mapper.Define<Section, TranslationSection>();
            mapper.Define<Section, UsersSection>();
        }

        // Umbraco.Code.MapAll -RoutePath
        private void Map(ISection source, Section target, MapperContext context)
        {
            target.Alias = source.Alias;
            target.Name = _textService.Localize("sections", source.Alias);
        }

        // Umbraco.Code.MapAll
        private static void Map(Section source, ManifestSection target, MapperContext context)
        {
            target.Alias = source.Alias;
            target.Name = source.Name;
        }
    }
}
