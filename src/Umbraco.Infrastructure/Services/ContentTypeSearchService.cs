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

    public ContentTypeSearchService(ISqlContext sqlContext, IContentTypeService contentTypeService)
    {
        _sqlContext = sqlContext;
        _contentTypeService = contentTypeService;
    }

    public async Task<PagedModel<IContentType>> SearchAsync(string query, bool? isElement, CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        // if the query is a GUID, search for that explicitly
        Guid.TryParse(query, out Guid guidQuery);

        IQuery<IContentType> nameQuery = isElement is not null ?
            _sqlContext.Query<IContentType>().Where(x => x.Name!.Contains(query) || x.Key == guidQuery || x.IsElement == isElement) :
            _sqlContext.Query<IContentType>().Where(x => x.Name!.Contains(query) || x.Key == guidQuery);

        IContentType[] contentTypes = (await _contentTypeService.GetByQueryAsync(nameQuery, cancellationToken)).ToArray();

        return new PagedModel<IContentType>
        {
            Items = contentTypes.Skip(skip).Take(take),
            Total = contentTypes.Count()
        };
    }
}
