using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface ILanguageFactory
{
    LanguageViewModel CreateLanguageViewModel(ILanguage language);

    ILanguage MapCreateModelToLanguage(LanguageCreateModel languageCreateModel);

    ILanguage MapUpdateModelToLanguage(ILanguage current, LanguageUpdateModel languageUpdateModel);
}
