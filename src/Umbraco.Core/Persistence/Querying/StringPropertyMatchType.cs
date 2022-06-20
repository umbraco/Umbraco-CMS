namespace Umbraco.Cms.Core.Persistence.Querying;

/// <summary>
///     Determines how to match a string property value
/// </summary>
public enum StringPropertyMatchType
{
    Exact,
    Contains,
    StartsWith,
    EndsWith,

    // Deals with % as wildcard chars in a string
    Wildcard,
}
