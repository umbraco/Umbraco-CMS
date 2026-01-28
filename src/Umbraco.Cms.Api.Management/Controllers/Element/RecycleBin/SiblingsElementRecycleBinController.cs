using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element.RecycleBin;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;

[ApiVersion("1.0")]
public class SiblingsElementRecycleBinController : ElementRecycleBinControllerBase
{
    public SiblingsElementRecycleBinController(
        IEntityService entityService,
        IElementPresentationFactory elementPresentationFactory)
        : base(entityService, elementPresentationFactory)
    {
    }

    [HttpGet("siblings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(SubsetViewModel<ElementRecycleBinItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubsetViewModel<ElementRecycleBinItemResponseModel>>> Siblings(CancellationToken cancellationToken, Guid target, int before, int after, Guid? dataTypeId = null)
        => await GetSiblings(target, before, after);
}
