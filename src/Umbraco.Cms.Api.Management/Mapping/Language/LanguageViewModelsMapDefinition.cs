using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Api.Management.ViewModels.Language;

namespace Umbraco.Cms.Api.Management.Mapping.Language;

public class LanguageViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<LanguageCreateModel, ILanguage>((_, _) => new Core.Models.Language(string.Empty, string.Empty), Map);
        mapper.Define<LanguageUpdateModel, ILanguage>((_, _) => new Core.Models.Language(string.Empty, string.Empty), Map);
        mapper.Define<ILanguage, LanguageViewModel>((_, _) => new LanguageViewModel(), Map);
    }

    // Umbraco.Code.MapAll -FallbackIsoCode
    private static void Map(ILanguage source, LanguageViewModel target, MapperContext context)
    {
        target.IsoCode = source.IsoCode;
        target.Name = source.CultureName;
        target.IsDefault = source.IsDefault;
        target.IsMandatory = source.IsMandatory;
    }

    // Umbraco.Code.MapAll -Id -FallbackLanguageId -Key
    private static void Map(LanguageCreateModel source, ILanguage target, MapperContext context)
    {
        target.CreateDate = default;
        if (!string.IsNullOrEmpty(source.Name))
        {
            target.CultureName = source.Name;
        }
        target.DeleteDate = null;
        target.IsDefault = source.IsDefault;
        target.IsMandatory = source.IsMandatory;
        target.IsoCode = source.IsoCode;
        target.UpdateDate = default;
    }

    // Umbraco.Code.MapAll -Id -FallbackLanguageId -Key -IsoCode -CreateDate
    private static void Map(LanguageUpdateModel source, ILanguage target, MapperContext context)
    {
        if (!string.IsNullOrEmpty(source.Name))
        {
            target.CultureName = source.Name;
        }
        target.DeleteDate = null;
        target.IsDefault = source.IsDefault;
        target.IsMandatory = source.IsMandatory;
        target.UpdateDate = default;
    }
}
