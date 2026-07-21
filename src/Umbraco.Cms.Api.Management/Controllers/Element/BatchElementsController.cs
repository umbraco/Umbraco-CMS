using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// Provides an API controller for retrieving the full details for multiple elements by key.
/// </summary>
[ApiVersion("1.0")]
public class BatchElementsController : ElementControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IElementService _elementService;
    private readonly IElementPresentationFactory _elementPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchElementsController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize access to element resources.</param>
    /// <param name="elementService">Service for retrieving element data within the CMS.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    public BatchElementsController(
        IAuthorizationService authorizationService,
        IElementService elementService,
        IElementPresentationFactory elementPresentationFactory)
    {
        _authorizationService = authorizationService;
        _elementService = elementService;
        _elementPresentationFactory = elementPresentationFactory;
    }

    [HttpGet("batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<ElementResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets multiple elements.")]
    [EndpointDescription("Gets multiple elements identified by the provided Ids.")]
    public async Task<IActionResult> Batch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        Guid[] requestedIds = [.. ids];

        if (requestedIds.Length == 0)
        {
            return Ok(new BatchResponseModel<ElementResponseModel>());
        }

        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementBrowse.ActionLetter, requestedIds),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        IEnumerable<IElement> elements = _elementService.GetByIds(requestedIds);
        IDictionary<Guid, IEnumerable<ContentSchedule>> schedulesByKey =
            _elementService.GetContentSchedulesByKeys(requestedIds);

        elements = OrderByRequestedIds(elements, requestedIds);

        var responseModels = elements
            .Select(element => CreateResponseModel(element, schedulesByKey))
            .ToList();

        return Ok(
            new BatchResponseModel<ElementResponseModel>
            {
                Total = responseModels.Count,
                Items = responseModels,
            });
    }

    private ElementResponseModel CreateResponseModel(IElement element, IDictionary<Guid, IEnumerable<ContentSchedule>> schedulesByKey)
    {
        var scheduleCollection = new ContentScheduleCollection();
        if (schedulesByKey.TryGetValue(element.Key, out IEnumerable<ContentSchedule>? schedules))
        {
            foreach (ContentSchedule schedule in schedules)
            {
                scheduleCollection.Add(schedule);
            }
        }

        return _elementPresentationFactory.CreateResponseModel(element, scheduleCollection);
    }
}
