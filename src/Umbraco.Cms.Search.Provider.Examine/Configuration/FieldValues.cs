namespace Umbraco.Cms.Search.Provider.Examine.Configuration;

/// <summary>
/// The type of Lucene field a property's values are indexed as.
/// </summary>
public enum FieldValues
{
    /// <summary>
    /// Full-text searchable text.
    /// </summary>
    Texts,

    /// <summary>
    /// Full-text searchable text at relevance level 1 (highest boost).
    /// </summary>
    TextsR1,

    /// <summary>
    /// Full-text searchable text at relevance level 2.
    /// </summary>
    TextsR2,

    /// <summary>
    /// Full-text searchable text at relevance level 3 (lowest boost).
    /// </summary>
    TextsR3,

    /// <summary>
    /// Integer values.
    /// </summary>
    Integers,

    /// <summary>
    /// Decimal values.
    /// </summary>
    Decimals,

    /// <summary>
    /// Date/time values.
    /// </summary>
    DateTimeOffsets,

    /// <summary>
    /// Exact-match keyword values.
    /// </summary>
    Keywords,
}
