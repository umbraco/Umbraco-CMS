using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;

[ApiVersion("1.0")]
public class SearchDocumentTypeTreeController : DocumentTypeTreeControllerBase
{
    private readonly IEntitySearchService _entitySearchService;
    private readonly IIdKeyMap _idKeyMap;

    public SearchDocumentTypeTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IContentTypeService contentTypeService,
        IEntitySearchService entitySearchService,
        IIdKeyMap idKeyMap)
        : base(entityService, flagProviders, contentTypeService)
    {
        _entitySearchService = entitySearchService;
        _idKeyMap = idKeyMap;
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<DocumentTypeTreeItemResponseModel>>> Search(CancellationToken cancellationToken, string? query, int skip = 0, int take = 100)
        => await SearchTreeEntities(_entitySearchService, _idKeyMap, query, skip, take);
}
