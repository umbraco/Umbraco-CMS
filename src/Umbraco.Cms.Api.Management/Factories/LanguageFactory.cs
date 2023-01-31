using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class LanguageFactory : ILanguageFactory
{
    // FIXME: use ILanguageService instead of ILocalizationService (pending language refactor to replace fallback language ID with fallback language IsoCode)
    private readonly ILocalizationService _localizationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public LanguageFactory(ILocalizationService localizationService, IUmbracoMapper umbracoMapper)
    {
        _localizationService = localizationService;
        _umbracoMapper = umbracoMapper;
    }

    public LanguageViewModel CreateLanguageViewModel(ILanguage language)
    {
        LanguageViewModel languageViewModel = _umbracoMapper.Map<LanguageViewModel>(language)!;
        if (language.FallbackLanguageId.HasValue)
        {
            languageViewModel.FallbackIsoCode = _localizationService.GetLanguageById(language.FallbackLanguageId.Value)?.IsoCode;
        }

        return languageViewModel;
    }

    public ILanguage MapCreateModelToLanguage(LanguageCreateModel languageCreateModel)
    {
        ILanguage created = _umbracoMapper.Map<ILanguage>(languageCreateModel)!;
        created.FallbackLanguageId = GetFallbackLanguageId(languageCreateModel);

        return created;
    }

    public ILanguage MapUpdateModelToLanguage(ILanguage current, LanguageUpdateModel languageUpdateModel)
    {
        ILanguage updated = _umbracoMapper.Map(languageUpdateModel, current);
        updated.FallbackLanguageId = GetFallbackLanguageId(languageUpdateModel);

        return updated;
    }

    private int? GetFallbackLanguageId(LanguageModelBase languageModelBase) =>
        string.IsNullOrWhiteSpace(languageModelBase.FallbackIsoCode)
            ? null
            : _localizationService.GetLanguageByIsoCode(languageModelBase.FallbackIsoCode)?.Id;
}
