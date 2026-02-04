using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Default implementation of <see cref="IApiDocumentUrlService"/> that retrieves document information by URL route.
/// </summary>
public sealed class ApiDocumentUrlService : IApiDocumentUrlService
{
    private readonly IDocumentUrlService _documentUrlService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiDocumentUrlService"/> class.
    /// </summary>
    /// <param name="documentUrlService">The document URL service.</param>
    public ApiDocumentUrlService(IDocumentUrlService documentUrlService)
        => _documentUrlService = documentUrlService;

    /// <inheritdoc />
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
