using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media.RecycleBin;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.RecycleBin;

    /// <summary>
    /// Controller responsible for handling operations related to sibling media items within the recycle bin.
    /// </summary>
[ApiVersion("1.0")]
public class SiblingsMediaRecycleBinController : MediaRecycleBinControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsMediaRecycleBinController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the recycle bin.</param>
    /// <param name="mediaPresentationFactory">Factory for creating media presentation models.</param>
    public SiblingsMediaRecycleBinController(IEntityService entityService, IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService, mediaPresentationFactory)
    {
    }

    /// <summary>
    /// Retrieves sibling media items in the recycle bin that are at the same hierarchical level as the specified media item.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="target">The unique identifier of the media item whose siblings are to be retrieved.</param>
    /// <param name="before">The number of sibling items to include before the target item.</param>
    /// <param name="after">The number of sibling items to include after the target item.</param>
    /// <param name="dataTypeId">An optional data type identifier to filter the sibling items.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="ActionResult{T}"/> with a <see cref="SubsetViewModel{MediaRecycleBinItemResponseModel}"/> of sibling media items.</returns>
    [HttpGet("siblings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(SubsetViewModel<MediaRecycleBinItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets sibling media in the recycle bin.")]
    [EndpointDescription("Gets a collection of sibling media items in the recycle bin at the same level as the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<MediaRecycleBinItemResponseModel>>> Siblings(CancellationToken cancellationToken, Guid target, int before, int after, Guid? dataTypeId = null)
        => await GetSiblings(target, before, after);
}
