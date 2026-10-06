using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.ElementVersion;

/// <summary>
/// Controller for retrieving a specific element version by its unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyElementVersionController : ElementVersionControllerBase
{
    private readonly IElementVersionService _elementVersionService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyElementVersionController"/> class.
    /// </summary>
    /// <param name="elementVersionService">Service for managing element versions.</param>
    /// <param name="umbracoMapper">Mapper for converting domain models to view models.</param>
    /// <param name="authorizationService">Service for handling authorization checks for the current user.</param>
    [ActivatorUtilitiesConstructor]
    public ByKeyElementVersionController(
        IElementVersionService elementVersionService,
        IUmbracoMapper umbracoMapper,
        IAuthorizationService authorizationService)
    {
        _elementVersionService = elementVersionService;
        _umbracoMapper = umbracoMapper;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyElementVersionController"/> class.
    /// </summary>
    /// <param name="elementVersionService">Service for managing element versions.</param>
    /// <param name="umbracoMapper">Mapper for converting domain models to view models.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 20.")]
    public ByKeyElementVersionController(
        IElementVersionService elementVersionService,
        IUmbracoMapper umbracoMapper)
        : this(
            elementVersionService,
            umbracoMapper,
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>())
    {
    }

    /// <summary>
    /// Retrieves a specific element version by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element version to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing the element version if found.</returns>
    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ElementVersionResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Gets a specific element version.")]
    [EndpointDescription("Gets a specific element version by its Id. If found, the result describes the version and includes details of the element type, editor, version date, and published status.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        Attempt<IElement?, ContentVersionOperationStatus> attempt =
            await _elementVersionService.GetAsync(id);
        if (attempt.Success is false || attempt.Result is null)
        {
            return MapFailure(attempt.Status);
        }

        IElement element = attempt.Result;
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementBrowse.ActionLetter, element.Key),
            AuthorizationPolicies.ElementPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        return Ok(_umbracoMapper.Map<ElementVersionResponseModel>(element));
    }
}
