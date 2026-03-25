using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// API controller responsible for handling operations related to the creation of elements in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class CreateElementController : CreateElementControllerBase
{
    private readonly IElementEditingPresentationFactory _elementEditingPresentationFactory;
    private readonly IElementEditingService _elementEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateElementController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize access to element creation operations.</param>
    /// <param name="elementEditingPresentationFactory">Factory for creating element editing presentation models.</param>
    /// <param name="elementEditingService">Service responsible for element editing functionality.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public CreateElementController(
        IAuthorizationService authorizationService,
        IElementEditingPresentationFactory elementEditingPresentationFactory,
        IElementEditingService elementEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : base(authorizationService)
    {
        _elementEditingPresentationFactory = elementEditingPresentationFactory;
        _elementEditingService = elementEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Creates a new element using the specified request model.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="requestModel">The details of the element to create.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a new element.")]
    [EndpointDescription("Creates a new element with the configuration specified in the request model.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, CreateElementRequestModel requestModel)
        => await HandleRequest(requestModel, async () =>
        {
            ElementCreateModel model = _elementEditingPresentationFactory.MapCreateModel(requestModel);
            Attempt<ElementCreateResult, ContentEditingOperationStatus> result =
                await _elementEditingService.CreateAsync(model, CurrentUserKey(_backOfficeSecurityAccessor));

            return result.Success
                ? CreatedAtId<ByKeyElementController>(controller => nameof(controller.ByKey), result.Result.Content!.Key)
                : ContentEditingOperationStatusResult(result.Status);
        });
}
