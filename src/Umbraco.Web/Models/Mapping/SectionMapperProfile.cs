using Umbraco.Core.Manifest;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models.Sections;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Sections;

namespace Umbraco.Web.Models.Mapping
{
    internal class SectionMapperProfile : IMapperProfile
    {
        private readonly ILocalizedTextService _textService;
        public SectionMapperProfile(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        public void SetMaps(Mapper mapper)
        {
            mapper.SetMap<ISection, Section>(source => new Section(), Map);

            // this is for AutoMapper ReverseMap - but really?
            mapper.SetMap<Section, ContentSection>();
            mapper.SetMap<Section, ContentSection>();
            mapper.SetMap<Section, ManifestSection>(Map);
            mapper.SetMap<Section, MediaSection>();
            mapper.SetMap<Section, MembersSection>();
            mapper.SetMap<Section, PackagesSection>();
            mapper.SetMap<Section, SettingsSection>();
            mapper.SetMap<Section, TranslationSection>();
            mapper.SetMap<Section, UsersSection>();
        }

        // Umbraco.Code.MapAll -RoutePath
        private void Map(ISection source, Section target)
        {
            target.Alias = source.Alias;
            target.Name = _textService.Localize("sections/" + source.Alias);
        }

        // Umbraco.Code.MapAll
        private static void Map(Section source, ManifestSection target)
        {
            target.Alias = source.Alias;
            target.Name = source.Name;
        }
    }
}
