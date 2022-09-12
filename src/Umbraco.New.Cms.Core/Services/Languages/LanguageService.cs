using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.New.Cms.Core.Services.Installer;

namespace Umbraco.New.Cms.Core.Services.Languages;

public class LanguageService : ILanguageService
{
    private readonly ILocalizationService _localizationService;

    public LanguageService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public bool LanguageAlreadyExists(int id, string isoCode)
    {
        // this is prone to race conditions but the service will not let us proceed anyways
        ILanguage? existingByCulture = _localizationService.GetLanguageByIsoCode(isoCode);

        // the localization service might return the generic language even when queried for specific ones (e.g. "da" when queried for "da-DK")
        // - we need to handle that explicitly
        if (existingByCulture?.IsoCode != isoCode)
        {
            existingByCulture = null;
        }

        if (existingByCulture != null && id != existingByCulture.Id)
        {
            return true;
        }

        ILanguage? existingById = id != default ? _localizationService.GetLanguageById(id) : null;
        return existingById is not null;
    }

    public bool CanUseLanguagesFallbackLanguage(ILanguage language)
    {
        if (!language.FallbackLanguageId.HasValue)
        {
            return false;
        }

        var languages = _localizationService.GetAllLanguages().ToDictionary(x => x.Id, x => x);
        return languages.ContainsKey(language.FallbackLanguageId.Value);

    }

    public bool CanGetProperFallbackLanguage(ILanguage existingById)
    {
        // modifying an existing language can create a fallback, verify
        // note that the service will check again, dealing with race conditions
        if (existingById.FallbackLanguageId.HasValue)
        {
            var languages = _localizationService.GetAllLanguages().ToDictionary(x => x.Id, x => x);

            if (CreatesCycle(existingById, languages))
            {
                return false;
            }
        }

        return true;
    }


    // see LocalizationService
    private bool CreatesCycle(ILanguage language, IDictionary<int, ILanguage> languages)
    {
        // a new language is not referenced yet, so cannot be part of a cycle
        if (!language.HasIdentity)
        {
            return false;
        }

        var id = language.FallbackLanguageId;
        while (true) // assuming languages does not already contains a cycle, this must end
        {
            if (!id.HasValue)
            {
                return false; // no fallback means no cycle
            }

            if (id.Value == language.Id)
            {
                return true; // back to language = cycle!
            }

            id = languages[id.Value].FallbackLanguageId; // else keep chaining
        }
    }
}
