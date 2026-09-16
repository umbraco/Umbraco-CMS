using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Parses HTML markup into indexable text, separating headings (H1-H3) from body text for relevance weighting.
/// </summary>
public interface IHtmlIndexValueParser
{
    /// <summary>
    /// Parses the given HTML into an index value, or null if it yields no indexable text.
    /// </summary>
    /// <param name="html">The HTML markup to parse.</param>
    /// <returns>The parsed index value, or null if no indexable text was found.</returns>
    IndexValue? Parse(string html);
}
