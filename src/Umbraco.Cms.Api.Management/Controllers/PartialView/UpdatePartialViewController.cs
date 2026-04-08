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
/// Controller responsible for handling HTTP requests to update partial views in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class UpdatePartialViewController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUmbracoMapper _mapper;
    private readonly IOptions<RuntimeSettings> _runtimeSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePartialViewController"/> class, which handles requests for updating partial views in the Umbraco backoffice.
    /// </summary>
    /// <param name="partialViewService">Service used to manage partial view files.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="mapper">The Umbraco object mapper for mapping between models.</param>
    /// <param name="runtimeSettings">The runtime configuration settings.</param>
    [ActivatorUtilitiesConstructor]
    public UpdatePartialViewController(
        IPartialViewService partialViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper mapper,
        IOptions<RuntimeSettings> runtimeSettings)
    {
        _partialViewService = partialViewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mapper = mapper;
        _runtimeSettings = runtimeSettings;
    }

    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public UpdatePartialViewController(
        IPartialViewService partialViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper mapper)
        : this(
            partialViewService,
            backOfficeSecurityAccessor,
            mapper,
            StaticServiceProvider.Instance.GetRequiredService<IOptions<RuntimeSettings>>())
    {
    }

    /// <summary>
    /// Updates an existing partial view identified by the specified path using the details provided in the request model.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="path">The virtual path identifying the partial view to update.</param>
    /// <param name="updateViewModel">The model containing the updated details for the partial view.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the asynchronous operation, with the result indicating the outcome of the update.</returns>
    [HttpPut("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a partial view.")]
    [EndpointDescription("Updates a partial view identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        string path,
        UpdatePartialViewRequestModel updateViewModel)
    {
        if (_runtimeSettings.Value.Mode == RuntimeMode.Production)
        {
            return PartialViewOperationStatusResult(PartialViewOperationStatus.NotAllowedInProductionMode);
        }

        path = DecodePath(path).VirtualPathToSystemPath();
        PartialViewUpdateModel updateModel = _mapper.Map<PartialViewUpdateModel>(updateViewModel)!;

        Attempt<IPartialView?, PartialViewOperationStatus> updateAttempt = await _partialViewService.UpdateAsync(path, updateModel, CurrentUserKey(_backOfficeSecurityAccessor));

        return updateAttempt.Success
            ? Ok()
            : PartialViewOperationStatusResult(updateAttempt.Status);
    }
}
