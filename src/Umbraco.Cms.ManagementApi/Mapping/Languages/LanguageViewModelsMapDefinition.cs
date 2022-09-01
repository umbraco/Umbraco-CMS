using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.ViewModels.Languages;

namespace Umbraco.Cms.ManagementApi.Mapping.Languages;

public class LanguageViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper) => mapper.Define<LanguageViewModel, Language>((source, context) => new Language(string.Empty, string.Empty), Map);


    // Umbraco.Code.MapAll
    private static void Map(LanguageViewModel source, Language target, MapperContext context)
    {
        target.CreateDate = default;
        target.CultureName = source.Name!;
        target.DeleteDate = null;
        target.FallbackLanguageId = source.FallbackLanguageId;
        target.Id = source.Id;
        target.IsDefault = source.IsDefault;
        target.IsMandatory = source.IsMandatory;
        target.IsoCode = source.IsoCode;
        target.Key = default;
        target.UpdateDate = default;
    }
}
