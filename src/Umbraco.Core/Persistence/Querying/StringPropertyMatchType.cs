namespace Umbraco.Cms.Core.Persistence.Querying;

/// <summary>
///     Determines how to match a string property value.
/// </summary>
public enum StringPropertyMatchType
{
    /// <summary>
    ///     Matches the exact value.
    /// </summary>
    Exact,

    /// <summary>
    ///     Matches if the property contains the value.
    /// </summary>
    Contains,

    /// <summary>
    ///     Matches if the property starts with the value.
    /// </summary>
    StartsWith,

    /// <summary>
    ///     Matches if the property ends with the value.
    /// </summary>
    EndsWith,

    /// <summary>
    ///     Matches using wildcard pattern, where % represents any sequence of characters.
    /// </summary>
    Wildcard,
}
