using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.PartialView;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
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
    private readonly IOptions<RuntimeSettings> _runtimeSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePartialViewController"/> class with the specified services.
    /// </summary>
    /// <param name="partialViewService">The service used to manage partial views.</param>
    /// <param name="umbracoMapper">The mapper used for Umbraco model mapping.</param>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security information.</param>
    /// <param name="runtimeSettings">The runtime configuration settings.</param>
    [ActivatorUtilitiesConstructor]
    public CreatePartialViewController(
        IPartialViewService partialViewService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IOptions<RuntimeSettings> runtimeSettings)
    {
        _partialViewService = partialViewService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _runtimeSettings = runtimeSettings;
    }

    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public CreatePartialViewController(
        IPartialViewService partialViewService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
            partialViewService,
            umbracoMapper,
            backOfficeSecurityAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IOptions<RuntimeSettings>>())
    {
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
        if (_runtimeSettings.Value.Mode == RuntimeMode.Production)
        {
            return PartialViewOperationStatusResult(PartialViewOperationStatus.NotAllowedInProductionMode);
        }

        PartialViewCreateModel createModel = _umbracoMapper.Map<PartialViewCreateModel>(requestModel)!;
        Attempt<IPartialView?, PartialViewOperationStatus> createAttempt = await _partialViewService.CreateAsync(createModel, CurrentUserKey(_backOfficeSecurityAccessor));

        return createAttempt.Success
            ? CreatedAtPath<ByPathPartialViewController>(controller => nameof(controller.ByPath), createAttempt.Result!.Path.SystemPathToVirtualPath())
            : PartialViewOperationStatusResult(createAttempt.Status);
    }
}
