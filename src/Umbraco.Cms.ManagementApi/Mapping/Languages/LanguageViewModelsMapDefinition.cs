using NPoco.FluentMappings;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.ViewModels.Language;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.ManagementApi.Mapping.Languages;

public class LanguageViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<LanguageViewModel, ILanguage>((source, context) => new Language(string.Empty, string.Empty), Map);
        mapper.Define<ILanguage, LanguageViewModel>((source, context) => new LanguageViewModel(), Map);
        mapper.Define<PagedModel<ILanguage>, PagedViewModel<LanguageViewModel>>((source, context) => new PagedViewModel<LanguageViewModel>(), Map);

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
    private static void Map(LanguageViewModel source, ILanguage target, MapperContext context)
    {
        target.CreateDate = default;
        if (!string.IsNullOrEmpty(source.Name))
        {
            target.CultureName = source.Name;
        }

        target.DeleteDate = null;
        target.FallbackLanguageId = source.FallbackLanguageId;
        target.Id = source.Id;
        target.IsDefault = source.IsDefault;
        target.IsMandatory = source.IsMandatory;
        target.IsoCode = source.IsoCode;
        target.Key = default;
        target.UpdateDate = default;
    }

    private static void Map(PagedModel<ILanguage> source, PagedViewModel<LanguageViewModel> target, MapperContext context)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (target is not PagedViewModel<LanguageViewModel> list)
        {
            throw new NotSupportedException($"{nameof(target)} must be a List<Language>.");
        }

        List<LanguageViewModel> temp = context.MapEnumerable<ILanguage, LanguageViewModel>(source.Items);

        // Put the default language first in the list & then sort rest by a-z
        LanguageViewModel? defaultLang = temp.SingleOrDefault(x => x.IsDefault);

        var languages = new List<LanguageViewModel>();

        // insert default lang first, then remaining language a-z
        if (defaultLang is not null)
        {
            languages.Add(defaultLang);
            languages.AddRange(temp.Where(x => x != defaultLang).OrderBy(x => x.Name));
        }

        list.Items = languages;
        list.Total = source.Total;
    }
}
