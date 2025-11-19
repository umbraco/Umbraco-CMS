using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Tree;

public class SiblingsElementTreeController : ElementTreeControllerBase
{
    public SiblingsElementTreeController(IEntityService entityService, IUmbracoMapper umbracoMapper, IElementPresentationFactory elementPresentationFactory)
        : base(entityService, umbracoMapper, elementPresentationFactory)
    {
    }

    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<ElementTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubsetViewModel<ElementTreeItemResponseModel>>> Siblings(
        CancellationToken cancellationToken,
        Guid target,
        int before,
        int after,
        bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetSiblings(target, before, after);
    }
}
