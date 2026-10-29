namespace Umbraco.Cms.Search.Provider.Examine.Services;

/// <summary>
/// Determines which free text query terms are stop words, i.e. common words the index analyzer leaves out of the index.
/// </summary>
/// <remarks>
/// Examine returns no results for a query that requires a term the analyzer did not index, so <see cref="Searcher"/>
/// makes stop words optional whenever the query has other terms to match on. Replace this service when the indexes
/// are analyzed with a different set of stop words.
/// </remarks>
[Obsolete("This is a workaround for Examine returning no results when a query requires a stop word, and will be removed once Examine handles this. Scheduled for removal in Umbraco 21.")]
public interface IStopWordDetector
{
    /// <summary>
    /// Determines whether the given query term is a stop word.
    /// </summary>
    /// <param name="term">The query term to check.</param>
    /// <returns><c>true</c> if the term is a stop word; otherwise <c>false</c>.</returns>
    bool IsStopWord(string term);
}
