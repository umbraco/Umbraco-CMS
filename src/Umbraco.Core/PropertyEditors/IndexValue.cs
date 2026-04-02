namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents an index value for a property to be indexed in the search system.
/// </summary>
public sealed class IndexValue
{
    /// <summary>
    ///     Gets or sets the culture for this index value.
    /// </summary>
    /// <remarks>
    ///     Can be <c>null</c> for culture-invariant properties.
    /// </remarks>
    public required string? Culture { get; set; }

    /// <summary>
    ///     Gets or sets the field name to use in the index.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    ///     Gets or sets the values to be indexed for this field.
    /// </summary>
    /// <remarks>
    ///     A field can have multiple values, for example when indexing tags or multi-select values.
    /// </remarks>
    public required IEnumerable<object?> Values { get; set; }
}
