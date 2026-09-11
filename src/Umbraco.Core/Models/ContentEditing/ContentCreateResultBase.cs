namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents the base result of a content creation operation.
/// </summary>
/// <typeparam name="TContent">The type of content being created, which must implement <see cref="IContentBase"/>.</typeparam>
public abstract class ContentCreateResultBase<TContent>
    where TContent : class, IContentBase
{
    /// <summary>
    ///     Gets the created content, or <c>null</c> if creation failed.
    /// </summary>
    public TContent? Content { get; init; }

    /// <summary>
    ///     Gets the validation result containing any validation errors that occurred during creation.
    /// </summary>
    public ContentValidationResult ValidationResult { get; init; } = new();

    /// <summary>
    ///     Gets the aliases of the properties that failed validation when publishing.
    /// </summary>
    /// <remarks>
    ///     Only populated when the operation included a publish that was blocked by invalid property values.
    /// </remarks>
    public IEnumerable<string> InvalidPropertyAliases { get; init; } = [];
}
