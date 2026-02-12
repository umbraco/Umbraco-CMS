namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

/// <summary>
///     Represents a composition relationship for a content type.
/// </summary>
public class Composition
{
    /// <summary>
    ///     Gets the unique key of the composed content type.
    /// </summary>
    public required Guid Key { get; init; }

    /// <summary>
    ///     Gets the type of composition relationship.
    /// </summary>
    public required CompositionType CompositionType { get; init; }
}
