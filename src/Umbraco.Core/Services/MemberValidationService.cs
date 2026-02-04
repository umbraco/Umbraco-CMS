using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Service for validating member content properties against their member type definitions.
/// </summary>
internal sealed class MemberValidationService : ContentValidationServiceBase<IMemberType>, IMemberValidationService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberValidationService" /> class.
    /// </summary>
    /// <param name="propertyValidationService">The property validation service.</param>
    /// <param name="languageService">The language service.</param>
    public MemberValidationService(IPropertyValidationService propertyValidationService, ILanguageService languageService)
        : base(propertyValidationService, languageService)
    {
    }

    /// <inheritdoc />
    public async Task<ContentValidationResult> ValidatePropertiesAsync(
        ContentEditingModelBase contentEditingModelBase,
        IMemberType memberType,
        IEnumerable<string?>? culturesToValidate = null)
        => await HandlePropertiesValidationAsync(contentEditingModelBase, memberType, culturesToValidate);
}
