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
    public SearchDocumentTypeTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IEntitySearchService entitySearchService,
        IIdKeyMap idKeyMap,
        IContentTypeService contentTypeService)
        : base(entityService, flagProviders, entitySearchService, idKeyMap, contentTypeService)
    {
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<DocumentTypeTreeItemResponseModel>>> Search(CancellationToken cancellationToken, string? query, int skip = 0, int take = 100, FolderOrItems folderOrItems = FolderOrItems.Both)
        => await SearchTreeEntities(query, skip, take, folderOrItems);
}
