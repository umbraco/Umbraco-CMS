using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Services;

internal sealed class MediaValidationService : ContentValidationServiceBase<IMediaType>, IMediaValidationService
{
    public MediaValidationService(IPropertyValidationService propertyValidationService, ILanguageService languageService)
        : base(propertyValidationService, languageService)
    {
    }

    public async Task<ContentValidationResult> ValidatePropertiesAsync(
        ContentEditingModelBase contentEditingModelBase,
        IMediaType mediaType)
        => await HandlePropertiesValidationAsync(contentEditingModelBase, mediaType);
}
