using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Api.Management.ViewModels.Language;

namespace Umbraco.Cms.Api.Management.Mapping.Language;

public class LanguageViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<CreateLanguageRequestModel, ILanguage>((_, _) => new Core.Models.Language(string.Empty, string.Empty), Map);
        mapper.Define<UpdateLanguageRequestModel, ILanguage>((_, _) => new Core.Models.Language(string.Empty, string.Empty), Map);
        mapper.Define<ILanguage, LanguageResponseModel>((_, _) => new LanguageResponseModel(), Map);
    }

    // Umbraco.Code.MapAll
    private static void Map(ILanguage source, LanguageResponseModel target, MapperContext context)
    {
        target.IsoCode = source.IsoCode;
        target.FallbackIsoCode = source.FallbackIsoCode;
        target.Name = source.CultureName;
        target.IsDefault = source.IsDefault;
        target.IsMandatory = source.IsMandatory;
    }

    // Umbraco.Code.MapAll -Id -Key
    private static void Map(CreateLanguageRequestModel source, ILanguage target, MapperContext context)
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
        target.FallbackIsoCode = source.FallbackIsoCode;
    }

    // Umbraco.Code.MapAll -Id -Key -IsoCode -CreateDate
    private static void Map(UpdateLanguageRequestModel source, ILanguage target, MapperContext context)
    {
        if (!string.IsNullOrEmpty(source.Name))
        {
            target.CultureName = source.Name;
        }
        target.DeleteDate = null;
        target.IsDefault = source.IsDefault;
        target.IsMandatory = source.IsMandatory;
        target.UpdateDate = default;
        target.FallbackIsoCode = source.FallbackIsoCode;
    }
}
