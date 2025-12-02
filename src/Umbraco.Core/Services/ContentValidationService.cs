using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Services;

internal sealed class ContentValidationService : ContentValidationServiceBase<IContentType>, IContentValidationService
{
    public ContentValidationService(IPropertyValidationService propertyValidationService, ILanguageService languageService, ISegmentService segmentService)
        : base(propertyValidationService, languageService, segmentService)
    {
    }

    public async Task<ContentValidationResult> ValidatePropertiesAsync(
        ContentEditingModelBase contentEditingModelBase,
        IContentType contentType,
        IEnumerable<string?>? culturesToValidate = null)
        => await HandlePropertiesValidationAsync(contentEditingModelBase, contentType, culturesToValidate);
}
