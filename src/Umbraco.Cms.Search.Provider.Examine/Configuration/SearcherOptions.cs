namespace Umbraco.Cms.Search.Provider.Examine.Configuration;

public sealed class SearcherOptions
{
    public int MaxFacetValues { get; set; } = 100;

    public float BoostFactorTextR1 { get; set; } = 6.0f;

    public float BoostFactorTextR2 { get; set; } = 4.0f;

    public float BoostFactorTextR3 { get; set; } = 2.0f;

    public bool ExpandFacetValues { get; set; } = false;
}
