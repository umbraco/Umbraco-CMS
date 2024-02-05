using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Querying.RecycleBin;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Media.RecycleBin;

[ApiVersion("1.0")]
public class OriginalParentMediaRecycleBinControllerBase : MediaRecycleBinControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediaPresentationFactory _mediaPresentationFactory;
    private readonly IMediaRecycleBinQueryService _mediaRecycleBinQueryService;

    public OriginalParentMediaRecycleBinControllerBase(
        IEntityService entityService,
        IAuthorizationService authorizationService,
        IMediaPresentationFactory mediaPresentationFactory,
        IMediaRecycleBinQueryService mediaRecycleBinQueryService)
        : base(entityService, mediaPresentationFactory)
    {
        _authorizationService = authorizationService;
        _mediaPresentationFactory = mediaPresentationFactory;
        _mediaRecycleBinQueryService = mediaRecycleBinQueryService;
    }

    [HttpGet("{id:guid}/original-parent")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MediaItemResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> OriginalParent(Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            MediaPermissionResource.RecycleBin(),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IMediaEntitySlim?, RecycleBinQueryResultType> getParentAttempt = await _mediaRecycleBinQueryService.GetOriginalParentAsync(id);
        if (getParentAttempt.Success is false)
        {
            return MapAttemptFailure(getParentAttempt.Status);
        }

        return getParentAttempt.Result is not null
            ? Ok(_mediaPresentationFactory.CreateItemResponseModel(getParentAttempt.Result))
            : Ok(null); // map this
    }

    private IActionResult MapAttemptFailure(RecycleBinQueryResultType status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            RecycleBinQueryResultType.NotFound => NotFound(problemDetailsBuilder
                .WithTitle("The media item could not be found")
                .Build()),
            RecycleBinQueryResultType.NotTrashed => BadRequest(problemDetailsBuilder
                .WithTitle("The media item is not trashed")
                .WithDetail("The media item needs to be trashed for the parent-before-recycled relation to be created.")
                .Build()),
            RecycleBinQueryResultType.NoParentRecycleRelation => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("The parent relation could not be found")
                .WithDetail("The relation between the parent and the media item that should have been created when the media item was deleted could not be found.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown recycle bin query type.")
                .Build()),
        });
}
