using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Services;

internal sealed class MemberValidationService : ContentValidationServiceBase<IMemberType>, IMemberValidationService
{
    public MemberValidationService(IPropertyValidationService propertyValidationService, ILanguageService languageService)
        : base(propertyValidationService, languageService)
    {
    }

    public async Task<ContentValidationResult> ValidatePropertiesAsync(
        ContentEditingModelBase contentEditingModelBase,
        IMemberType memberType,
        IEnumerable<string?>? culturesToValidate = null)
        => await HandlePropertiesValidationAsync(contentEditingModelBase, memberType, culturesToValidate);
}
