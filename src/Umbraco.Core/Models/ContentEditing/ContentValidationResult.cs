using Umbraco.Cms.Core.Models.ContentEditing.Validation;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents the result of content validation containing any validation errors.
/// </summary>
public class ContentValidationResult
{
    /// <summary>
    ///     Gets the collection of property validation errors that occurred during validation.
    /// </summary>
    public IEnumerable<PropertyValidationError> ValidationErrors { get; init; } = Enumerable.Empty<PropertyValidationError>();
}
