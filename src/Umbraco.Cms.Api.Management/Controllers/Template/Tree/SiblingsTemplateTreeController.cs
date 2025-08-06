using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Tree;

public class SiblingsTemplateTreeController : TemplateTreeControllerBase
{
    public SiblingsTemplateTreeController(IEntityService entityService)
        : base(entityService)
    {
    }

    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    public Task<ActionResult<SubsetViewModel<NamedEntityTreeItemResponseModel>>> Siblings(
        CancellationToken cancellationToken,
        Guid target,
        int before,
        int after) =>
        GetSiblings(target, before, after);
}
