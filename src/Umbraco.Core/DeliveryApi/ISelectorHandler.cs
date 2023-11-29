namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A handler that handles fetch query parameter.
/// </summary>
public interface ISelectorHandler : IQueryHandler
{
    /// <summary>
    ///     Builds a <see cref="SelectorOption"/> for the selector query.
    /// </summary>
    /// <param name="selector">The selector query (i.e. "children:articles").</param>
    /// <returns>A <see cref="SelectorOption"/> that can be used when building specific search query for requesting a subset of the items.</returns>
    SelectorOption BuildSelectorOption(string selector);
}
