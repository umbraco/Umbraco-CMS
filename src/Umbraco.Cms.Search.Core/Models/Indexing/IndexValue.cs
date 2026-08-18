namespace Umbraco.Cms.Search.Core.Models.Indexing;

/// <summary>
/// Represents the typed value(s) to index for a single field. Only the members relevant to the field's type are populated.
/// </summary>
public record IndexValue
{
    /// <summary>
    /// Texts with the highest degree of relevance when scoring search results.
    /// </summary>
    public IEnumerable<string>? TextsR1 { get; init; }

    /// <summary>
    /// Texts with the second-highest degree of relevance when scoring search results.
    /// </summary>
    public IEnumerable<string>? TextsR2 { get; init; }

    /// <summary>
    /// Texts with the third-highest degree of relevance when scoring search results.
    /// </summary>
    public IEnumerable<string>? TextsR3 { get; init; }

    /// <summary>
    /// Texts with the lowest degree of relevance when scoring search results.
    /// </summary>
    public IEnumerable<string>? Texts { get; init; }

    /// <summary>
    /// Exact-match keyword values.
    /// </summary>
    public IEnumerable<string>? Keywords { get; init; }

    /// <summary>
    /// Integer values.
    /// </summary>
    public IEnumerable<int>? Integers { get; init; }

    /// <summary>
    /// Decimal values.
    /// </summary>
    public IEnumerable<decimal>? Decimals { get; init; }

    /// <summary>
    /// Date/time values.
    /// </summary>
    public IEnumerable<DateTimeOffset>? DateTimeOffsets { get; init; }
}
