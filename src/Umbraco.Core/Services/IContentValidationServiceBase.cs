using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides a base interface for content validation services.
/// </summary>
/// <typeparam name="TContentType">The type of content type used for validation.</typeparam>
internal interface IContentValidationServiceBase<in TContentType>
    where TContentType : IContentTypeComposition
{
    /// <summary>
    ///     Validates the properties of a content editing model against its content type definition.
    /// </summary>
    /// <param name="contentEditingModelBase">The content editing model to validate.</param>
    /// <param name="contentType">The content type definition to validate against.</param>
    /// <param name="culturesToValidate">The optional collection of cultures to validate. If <c>null</c>, all cultures are validated.</param>
    /// <returns>A validation result containing any validation errors.</returns>
    Task<ContentValidationResult> ValidatePropertiesAsync(ContentEditingModelBase contentEditingModelBase, TContentType contentType, IEnumerable<string?>? culturesToValidate = null);

    /// <summary>
    ///     Validates that the cultures specified in the content editing model are valid.
    /// </summary>
    /// <param name="contentEditingModelBase">The content editing model to validate.</param>
    /// <returns><c>true</c> if the cultures are valid; otherwise, <c>false</c>.</returns>
    Task<bool> ValidateCulturesAsync(ContentEditingModelBase contentEditingModelBase);
}
