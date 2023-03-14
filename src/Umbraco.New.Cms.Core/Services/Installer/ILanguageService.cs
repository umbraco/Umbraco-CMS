using Umbraco.Cms.Core.Models;

namespace Umbraco.New.Cms.Core.Services.Installer;

public interface ILanguageService
{
    bool LanguageAlreadyExists(int id, string isoCode);

    bool CanUseLanguagesFallbackLanguage(ILanguage language);
    bool CanGetProperFallbackLanguage(ILanguage existingById);
}
