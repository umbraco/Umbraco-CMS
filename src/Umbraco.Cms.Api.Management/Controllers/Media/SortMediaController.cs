using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.Media;
using Umbraco.Cms.Api.Management.ViewModels.Sorting;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

/// <summary>
/// Provides API endpoints for sorting media items within the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class SortMediaController : MediaControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SortMediaController"/> class, which handles sorting operations for media items in the Umbraco backoffice.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    /// <param name="mediaEditingService">Service responsible for editing and sorting media items.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for backoffice security context.</param>
    public SortMediaController(
        IAuthorizationService authorizationService,
        IMediaEditingService mediaEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _mediaEditingService = mediaEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Sorts media items within a specified parent container based on the provided sort order.
    /// </summary>
    /// <param name="cancellationToken">Token to observe while waiting for the task to complete.</param>
    /// <param name="sortingRequestModel">The model containing the parent container ID and the desired sort order for its child media items.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the sort operation.</returns>
    [HttpPut("sort")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Sorts media items.")]
    [EndpointDescription("Sorts media items in the specified parent container according to the provided sort order.")]
    public async Task<IActionResult> Sort(CancellationToken cancellationToken, SortingRequestModel sortingRequestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            MediaPermissionResource.WithKeys(new List<Guid?>(sortingRequestModel.Sorting.Select(x => x.Id).Cast<Guid?>()) { sortingRequestModel.Parent?.Id }),
            AuthorizationPolicies.MediaPermissionByResource);
        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        ContentEditingOperationStatus result = await _mediaEditingService.SortAsync(
            sortingRequestModel.Parent?.Id,
            sortingRequestModel.Sorting.Select(m => new SortingModel { Key = m.Id, SortOrder = m.SortOrder }),
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result == ContentEditingOperationStatus.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result);
    }
}
