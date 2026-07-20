using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides validation services for content (document) properties and cultures.
/// </summary>
internal sealed class ContentValidationService : ContentValidationServiceBase<IContentType>, IContentValidationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentValidationService"/> class.
    /// </summary>
    /// <param name="propertyValidationService">The property validation service.</param>
    /// <param name="languageService">The language service.</param>
    public ContentValidationService(IPropertyValidationService propertyValidationService, ILanguageService languageService)
        : base(propertyValidationService, languageService)
    {
    }

    /// <inheritdoc />
    public async Task<ContentValidationResult> ValidatePropertiesAsync(
        ContentEditingModelBase contentEditingModelBase,
        IContentType contentType,
        IEnumerable<string?>? culturesToValidate = null)
        => await HandlePropertiesValidationAsync(contentEditingModelBase, contentType, culturesToValidate);
}
