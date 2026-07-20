using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Service for validating media content properties against their media type definitions.
/// </summary>
internal sealed class MediaValidationService : ContentValidationServiceBase<IMediaType>, IMediaValidationService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaValidationService" /> class.
    /// </summary>
    /// <param name="propertyValidationService">The property validation service.</param>
    /// <param name="languageService">The language service.</param>
    public MediaValidationService(IPropertyValidationService propertyValidationService, ILanguageService languageService)
        : base(propertyValidationService, languageService)
    {
    }

    /// <inheritdoc />
    public async Task<ContentValidationResult> ValidatePropertiesAsync(
        ContentEditingModelBase contentEditingModelBase,
        IMediaType mediaType,
        IEnumerable<string?>? culturesToValidate = null)
        => await HandlePropertiesValidationAsync(contentEditingModelBase, mediaType, culturesToValidate);
}
