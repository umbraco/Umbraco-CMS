using Examine;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Content.Services;

public interface IApiQueryExtensionService
{
    /// <summary>
    ///     Gets a collection of Guid from querying the <see cref="Constants.UmbracoIndexes.ContentAPIIndexName"/>.
    /// </summary>
    /// <param name="fieldName">The name of the field to query.</param>
    /// <param name="id">The id value to query for.</param>
    /// <param name="processResults">A function to process the obtained search results.</param>
    /// <returns>The result from applying the <paramref name="processResults"/> on the search results.</returns>
    IEnumerable<Guid> GetGuidsFromResults(string fieldName, Guid id, Func<ISearchResults, IEnumerable<Guid>> processResults);
}
