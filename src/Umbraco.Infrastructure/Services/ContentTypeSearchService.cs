using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Services;

/// <inheritdoc />
internal sealed class ContentTypeSearchService : IContentTypeSearchService
{
    private readonly ISqlContext _sqlContext;
    private readonly IContentTypeService _contentTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeSearchService"/> class.
    /// </summary>
    /// <param name="sqlContext">The SQL context for querying the database.</param>
    /// <param name="contentTypeService">The content type service for retrieving content types.</param>
    public ContentTypeSearchService(ISqlContext sqlContext, IContentTypeService contentTypeService)
    {
        _sqlContext = sqlContext;
        _contentTypeService = contentTypeService;
    }

    /// <inheritdoc/>
    public async Task<PagedModel<IContentType>> SearchAsync(
        string? query,
        bool? isElement,
        bool? allowedInLibrary,
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        IQuery<IContentType> contentTypeQuery = _sqlContext.Query<IContentType>();

        if (string.IsNullOrEmpty(query) is false)
        {
            // if the query is a GUID, search for that explicitly
            Guid.TryParse(query, out Guid guidQuery);
            contentTypeQuery = contentTypeQuery.Where(x => x.Name!.Contains(query) || x.Key == guidQuery);
        }

        if (isElement is not null)
        {
            contentTypeQuery = contentTypeQuery.Where(x => x.IsElement == isElement);
        }

        if (allowedInLibrary is not null)
        {
            contentTypeQuery = contentTypeQuery.Where(x => x.AllowedInLibrary == allowedInLibrary);
        }

        IContentType[] contentTypes =
            (await _contentTypeService.GetByQueryAsync(contentTypeQuery, cancellationToken)).ToArray();
        return new PagedModel<IContentType>
        {
            Items = contentTypes.Skip(skip).Take(take),
            Total = contentTypes.Length,
        };
    }
}
