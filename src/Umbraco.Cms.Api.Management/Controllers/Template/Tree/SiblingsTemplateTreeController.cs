using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Tree;

public class SiblingsTemplateTreeController : TemplateTreeControllerBase
{
    public SiblingsTemplateTreeController(IEntityService entityService, FlagProviderCollection flagProviders)
        : base(entityService, flagProviders)
    {
    }


    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of template tree sibling items.")]
    [EndpointDescription("Gets a collection of template tree items that are siblings of the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<NamedEntityTreeItemResponseModel>>> Siblings(
        CancellationToken cancellationToken,
        Guid target,
        int before,
        int after) =>
        await GetSiblings(target, before, after);
}
