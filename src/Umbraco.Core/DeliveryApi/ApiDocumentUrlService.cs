using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ApiDocumentUrlService : IApiDocumentUrlService
{
    private readonly IDocumentUrlService _documentUrlService;

    public ApiDocumentUrlService(IDocumentUrlService documentUrlService)
        => _documentUrlService = documentUrlService;

    public Guid? GetDocumentKeyByRoute(string route, string? culture, bool preview)
    {
        // Handle the nasty logic with domain document ids in front of paths.
        int? documentStartNodeId = null;
        if (route.StartsWith('/') is false)
        {
            var index = route.IndexOf('/');

            if (index > -1 && int.TryParse(route.Substring(0, index), out var nodeId))
            {
                documentStartNodeId = nodeId;
                route = route.Substring(index);
            }
        }

        return _documentUrlService.GetDocumentKeyByRoute(
            route,
            culture.NullOrWhiteSpaceAsNull(),
            documentStartNodeId,
            preview);
    }
}
