using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Sections;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

public class SectionMapDefinition : IMapDefinition
{
    private readonly ILocalizedTextService _textService;

    public SectionMapDefinition(ILocalizedTextService textService) => _textService = textService;

    public void DefineMaps(IUmbracoMapper mapper)
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

    // Umbraco.Code.MapAll
    private static void Map(Section source, ManifestSection target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.Name = source.Name;
    }

    // Umbraco.Code.MapAll -RoutePath
    private void Map(ISection source, Section target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.Name = _textService.Localize("sections", source.Alias);
    }
}
