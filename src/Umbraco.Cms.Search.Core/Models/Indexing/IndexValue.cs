namespace Umbraco.Cms.Search.Core.Models.Indexing;

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

    public IEnumerable<string>? Keywords { get; init; }

    public IEnumerable<int>? Integers { get; init; }

    public IEnumerable<decimal>? Decimals { get; init; }

    public IEnumerable<DateTimeOffset>? DateTimeOffsets { get; init; }
}
