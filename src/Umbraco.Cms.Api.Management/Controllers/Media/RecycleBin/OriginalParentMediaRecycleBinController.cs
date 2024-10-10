using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Media;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Querying.RecycleBin;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Media.RecycleBin;

[ApiVersion("1.0")]
public class OriginalParentMediaRecycleBinController : MediaRecycleBinControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediaPresentationFactory _mediaPresentationFactory;
    private readonly IMediaRecycleBinQueryService _mediaRecycleBinQueryService;

    public OriginalParentMediaRecycleBinController(
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
    [ProducesResponseType(typeof(ReferenceByIdModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OriginalParent(
        CancellationToken cancellationToken,
        Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            MediaPermissionResource.RecycleBin(),
            AuthorizationPolicies.MediaPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IMediaEntitySlim?, RecycleBinQueryResultType> getParentAttempt = await _mediaRecycleBinQueryService.GetOriginalParentAsync(id);
        return getParentAttempt.Success switch
        {
            true when getParentAttempt.Status == RecycleBinQueryResultType.Success
                => Ok(new ReferenceByIdModel(getParentAttempt.Result!.Key)),
            true when getParentAttempt.Status == RecycleBinQueryResultType.ParentIsRoot
                => Ok(null),
            _ => MapAttemptFailure(getParentAttempt.Status),
        };
    }

    private IActionResult MapAttemptFailure(RecycleBinQueryResultType status)
        => MapRecycleBinQueryAttemptFailure(status, "media item");
}
