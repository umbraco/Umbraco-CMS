using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Tree;

public class SiblingsTreeController : DocumentBlueprintTreeControllerBase
{
    public SiblingsTreeController(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory) : base(entityService, documentPresentationFactory)
    {
    }

    [HttpGet("siblings")]
    [ProducesResponseType(typeof(IEnumerable<DocumentBlueprintTreeItemResponseModel>), StatusCodes.Status200OK)]
    public Task<ActionResult<IEnumerable<DocumentBlueprintTreeItemResponseModel>>> Siblings(
        CancellationToken cancellationToken,
        Guid target,
        int before,
        int after) =>
        GetSiblings(target, before, after);
}
