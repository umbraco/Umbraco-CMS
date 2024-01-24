using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Services;

internal sealed class MediaValidationService : ContentValidationServiceBase<IMediaType>, IMediaValidationService
{
    public MediaValidationService(
        PropertyEditorCollection propertyEditorCollection,
        ILanguageService languageService,
        ILogger<MediaValidationService> logger)
        : base(propertyEditorCollection, languageService, logger)
    {
    }

    public async Task<ContentValidationResult> ValidatePropertiesAsync(
        ContentEditingModelBase contentEditingModelBase,
        IMediaType mediaType)
        => await HandlePropertiesValidationAsync(contentEditingModelBase, mediaType);
}
