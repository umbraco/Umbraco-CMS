using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Tree;

[ApiVersion("1.0")]
public class AncestorsTemplateTreeController : TemplateTreeControllerBase
{
    public AncestorsTemplateTreeController(IEntityService entityService)
        : base(entityService)
    {
    }

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NamedEntityTreeItemResponseModel>>> Ancestors(
        CancellationToken cancellationToken,
        Guid descendantId)
        => await GetAncestors(descendantId);
}
