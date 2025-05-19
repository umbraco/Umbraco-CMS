using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IContentTypeSearchService
{
    Task<PagedModel<IContentType>> SearchAsync(string query, bool? isElement, CancellationToken cancellationToken, int skip = 0, int take = 100);
}
