using Lucene.Net.Analysis.Standard;

namespace Umbraco.Cms.Search.Provider.Examine.Services;

/// <summary>
/// The default <see cref="IStopWordDetector"/>, using the stop words of Lucene's <see cref="StandardAnalyzer"/>.
/// </summary>
[Obsolete("This is a workaround for Examine returning no results when a query requires a stop word, and will be removed once Examine handles this. Scheduled for removal in Umbraco 21.")]
public class StopWordDetector : IStopWordDetector
{
    /// <inheritdoc />
    public bool IsStopWord(string term)
        => StandardAnalyzer.STOP_WORDS_SET.Contains(term.ToLowerInvariant());
}
