namespace Umbraco.Cms.Core.Models.Search;

public interface IUmbracoSearchResults : IEnumerable<IUmbracoSearchResult>
{
    /// <summary>
    /// Returns the Total item count for the search regardless of skip/take/max count values
    /// </summary>
    long TotalItemCount { get; }
}
