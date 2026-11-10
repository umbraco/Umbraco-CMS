using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Services;

internal sealed class ElementValidationService : ContentValidationServiceBase<IContentType>, IElementValidationService
{
    public ElementValidationService(IPropertyValidationService propertyValidationService, ILanguageService languageService)
        : base(propertyValidationService, languageService)
    {
    }

    public async Task<ContentValidationResult> ValidatePropertiesAsync(
        ContentEditingModelBase contentEditingModelBase,
        IContentType contentType,
        IEnumerable<string?>? culturesToValidate = null,
        bool validateCultureInvariantProperties = true)
        => await HandlePropertiesValidationAsync(contentEditingModelBase, contentType, culturesToValidate, validateCultureInvariantProperties);
}
