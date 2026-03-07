using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.PartialView;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView;

/// <summary>
/// API controller responsible for handling requests to create new partial views in the CMS.
/// </summary>
[ApiVersion("1.0")]
public class CreatePartialViewController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

/// <summary>
/// Initializes a new instance of the <see cref="CreatePartialViewController"/> class with the specified services.
/// </summary>
/// <param name="partialViewService">The service used to manage partial views.</param>
/// <param name="umbracoMapper">The mapper used for Umbraco model mapping.</param>
/// <param name="backOfficeSecurityAccessor">Provides access to back office security information.</param>
    public CreatePartialViewController(
        IPartialViewService partialViewService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _partialViewService = partialViewService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Creates a new partial view based on the provided request model.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="requestModel">Details of the partial view to create.</param>
    /// <returns>An <see cref="IActionResult"/> representing the outcome of the creation operation.</returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a new partial view.")]
    [EndpointDescription("Creates a new partial view with the configuration specified in the request model.")]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreatePartialViewRequestModel requestModel)
    {
        PartialViewCreateModel createModel = _umbracoMapper.Map<PartialViewCreateModel>(requestModel)!;
        Attempt<IPartialView?, PartialViewOperationStatus> createAttempt = await _partialViewService.CreateAsync(createModel, CurrentUserKey(_backOfficeSecurityAccessor));

        return createAttempt.Success
            ? CreatedAtPath<ByPathPartialViewController>(controller => nameof(controller.ByPath), createAttempt.Result!.Path.SystemPathToVirtualPath())
            : PartialViewOperationStatusResult(createAttempt.Status);
    }
}
