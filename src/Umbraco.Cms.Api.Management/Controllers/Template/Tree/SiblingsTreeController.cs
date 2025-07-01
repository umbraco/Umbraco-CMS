using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Tree;

public class SiblingsTreeController : TemplateTreeControllerBase
{
    public SiblingsTreeController(IEntityService entityService) : base(entityService)
    {
    }

    [HttpGet("siblings")]
    [ProducesResponseType(typeof(IEnumerable<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    public Task<ActionResult<IEnumerable<NamedEntityTreeItemResponseModel>>> Siblings(
        CancellationToken cancellationToken,
        Guid target,
        int before,
        int after) =>
        GetSiblings(target, before, after);
}
