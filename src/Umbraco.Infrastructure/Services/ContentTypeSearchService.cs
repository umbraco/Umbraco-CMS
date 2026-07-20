using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Services;

internal sealed class ContentTypeSearchService : IContentTypeSearchService
{
    private readonly ISqlContext _sqlContext;
    private readonly IContentTypeService _contentTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeSearchService"/> class.
    /// </summary>
    /// <param name="sqlContext">Provides database context for SQL operations.</param>
    /// <param name="contentTypeService">Service for managing content types.</param>
    public ContentTypeSearchService(ISqlContext sqlContext, IContentTypeService contentTypeService)
    {
        _sqlContext = sqlContext;
        _contentTypeService = contentTypeService;
    }

    /// <summary>
    /// Asynchronously searches for content types whose names contain the specified query string or whose key matches the query as a GUID.
    /// Optionally filters results to only include content types that are elements.
    /// </summary>
    /// <param name="query">The search string to match against content type names or keys (as GUID).</param>
    /// <param name="isElement">If specified, filters results to content types where <c>IsElement</c> matches this value.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of matching items to skip before returning results (for paging).</param>
    /// <param name="take">The maximum number of items to return (for paging).</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains a <see cref="PagedModel{IContentType}"/> with the matching content types and the total count.
    /// </returns>
    public async Task<PagedModel<IContentType>> SearchAsync(string query, bool? isElement, CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        // if the query is a GUID, search for that explicitly
        Guid.TryParse(query, out Guid guidQuery);

        IQuery<IContentType> nameQuery = isElement is not null ?
            _sqlContext.Query<IContentType>().Where(x => (x.Name!.Contains(query) || x.Key == guidQuery) && x.IsElement == isElement) :
            _sqlContext.Query<IContentType>().Where(x => x.Name!.Contains(query) || x.Key == guidQuery);

        IContentType[] contentTypes = (await _contentTypeService.GetByQueryAsync(nameQuery, cancellationToken)).ToArray();

        return new PagedModel<IContentType>
        {
            Items = contentTypes.Skip(skip).Take(take),
            Total = contentTypes.Count()
        };
    }
}
