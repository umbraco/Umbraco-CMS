using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media.RecycleBin;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.RecycleBin;

[ApiVersion("1.0")]
public class SiblingsMediaRecycleBinController : MediaRecycleBinControllerBase
{
    public SiblingsMediaRecycleBinController(IEntityService entityService, IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService, mediaPresentationFactory)
    {
    }

    [HttpGet("siblings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(SubsetViewModel<MediaRecycleBinItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubsetViewModel<MediaRecycleBinItemResponseModel>>> Siblings(CancellationToken cancellationToken, Guid target, int before, int after, Guid? dataTypeId = null)
        => await GetSiblings(target, before, after);
}
