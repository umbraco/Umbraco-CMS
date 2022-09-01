using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.ViewModels.Languages;

namespace Umbraco.Cms.ManagementApi.Mapping.Languages;

public class LanguageViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<LanguageViewModel, Language>((source, context) => new Language(string.Empty, string.Empty), Map);
        mapper.Define<ILanguage, LanguageViewModel>((source, context) => new LanguageViewModel(), Map);
        mapper.Define<IEnumerable<ILanguage>, IEnumerable<LanguageViewModel>>((source, context) => new List<LanguageViewModel>(), Map);

    }

    // Umbraco.Code.MapAll
    private static void Map(ILanguage source, LanguageViewModel target, MapperContext context)
    {
        target.Id = source.Id;
        target.IsoCode = source.IsoCode;
        target.Name = source.CultureName;
        target.IsDefault = source.IsDefault;
        target.IsMandatory = source.IsMandatory;
        target.FallbackLanguageId = source.FallbackLanguageId;
    }


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

    private static void Map(IEnumerable<ILanguage> source, IEnumerable<LanguageViewModel> target, MapperContext context)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (target is not List<LanguageViewModel> list)
        {
            throw new NotSupportedException($"{nameof(target)} must be a List<Language>.");
        }

        List<LanguageViewModel> temp = context.MapEnumerable<ILanguage, LanguageViewModel>(source);

        // Put the default language first in the list & then sort rest by a-z
        LanguageViewModel? defaultLang = temp.SingleOrDefault(x => x.IsDefault);

        // insert default lang first, then remaining language a-z
        if (defaultLang is not null)
        {
            list.Add(defaultLang);
            list.AddRange(temp.Where(x => x != defaultLang).OrderBy(x => x.Name));
        }
    }
}
