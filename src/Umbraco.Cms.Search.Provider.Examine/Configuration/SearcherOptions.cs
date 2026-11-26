namespace Umbraco.Cms.Search.Provider.Examine.Configuration;

/// <summary>
/// Configures search-time behavior for the Examine search provider.
/// </summary>
public sealed class SearcherOptions
{
    /// <summary>
    /// Gets or sets the maximum number of values returned per facet.
    /// </summary>
    public int MaxFacetValues { get; set; } = 100;

    /// <summary>
    /// Gets or sets the relevance boost applied to relevance-level-1 text matches.
    /// </summary>
    public float BoostFactorTextR1 { get; set; } = 6.0f;

    /// <summary>
    /// Gets or sets the relevance boost applied to relevance-level-2 text matches.
    /// </summary>
    public float BoostFactorTextR2 { get; set; } = 4.0f;

    /// <summary>
    /// Gets or sets the relevance boost applied to relevance-level-3 text matches.
    /// </summary>
    public float BoostFactorTextR3 { get; set; } = 2.0f;

    /// <summary>
    /// Gets or sets a value indicating whether facets should be re-queried once per applied filter, so a facet's
    /// own selected value is not excluded from its result count.
    /// </summary>
    public bool ExpandFacetValues { get; set; } = false;
}
