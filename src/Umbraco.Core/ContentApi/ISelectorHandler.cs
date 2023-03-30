namespace Umbraco.Cms.Core.ContentApi;

/// <summary>
///     A handler that handles fetch query parameter.
/// </summary>
public interface ISelectorHandler : IQueryHandler
{
    /// <summary>
    ///     Builds a <see cref="SelectorOption"/> for the fetch query parameter.
    /// </summary>
    /// <param name="selectorValueString">The selector (fetch) value from the query string.</param>
    /// <returns>A <see cref="SelectorOption"/> that can be used when building specific search query for requesting a subset of the items.</returns>
    SelectorOption BuildSelectorOption(string selectorValueString);
}
