using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentEditing.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services.OperationStatus;

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

    public async Task<Attempt<IList<PropertyValidationError>, ContentEditingOperationStatus>> ValidatePropertiesAsync(
        ContentEditingModelBase contentEditingModelBase,
        IMediaType mediaType)
        => await HandlePropertiesValidationAsync(contentEditingModelBase, mediaType);
}
