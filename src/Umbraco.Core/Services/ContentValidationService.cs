using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentEditing.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

internal sealed class ContentValidationService : ContentValidationServiceBase<IContentType>, IContentValidationService
{
    public ContentValidationService(
        PropertyEditorCollection propertyEditorCollection,
        ILanguageService languageService,
        ILogger<ContentValidationService> logger)
        : base(propertyEditorCollection, languageService, logger)
    {
    }

    public async Task<Attempt<IList<PropertyValidationError>, ContentEditingOperationStatus>> ValidatePropertiesAsync(
        ContentEditingModelBase contentEditingModelBase,
        IContentType contentType)
        => await HandlePropertiesValidationAsync(contentEditingModelBase, contentType);
}
