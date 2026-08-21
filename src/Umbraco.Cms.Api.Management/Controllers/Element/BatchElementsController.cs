using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// Provides an API controller for retrieving the full details for multiple elements by key.
/// </summary>
[ApiVersion("1.0")]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Element)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Element))]
public class BatchElementsController : ManagementApiControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IElementPermissionService _elementPermissionService;
    private readonly IElementService _elementService;
    private readonly IElementPresentationFactory _elementPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchElementsController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to the current backoffice user's security context.</param>
    /// <param name="elementPermissionService">Service used to authorize access to element resources.</param>
    /// <param name="elementService">Service for retrieving element data within the CMS.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    public BatchElementsController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IElementPermissionService elementPermissionService,
        IElementService elementService,
        IElementPresentationFactory elementPresentationFactory)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _elementPermissionService = elementPermissionService;
        _elementService = elementService;
        _elementPresentationFactory = elementPresentationFactory;
    }

    /// <remarks>
    /// Ids the current user is not authorized to browse are silently omitted rather than failing the request -
    /// this endpoint is not gated on Library section access, so a user without it simply receives no items.
    /// </remarks>
    [HttpGet("batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<ElementResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets multiple elements.")]
    [EndpointDescription("Gets multiple elements identified by the provided Ids. Ids the current user is not authorized to browse are omitted from the result.")]
    public async Task<IActionResult> Batch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        Guid[] requestedIds = [.. ids];

        if (requestedIds.Length == 0)
        {
            return Ok(new BatchResponseModel<ElementResponseModel>());
        }

        IUser currentUser = CurrentUser(_backOfficeSecurityAccessor);

        ISet<Guid> authorizedIds = await _elementPermissionService.FilterAuthorizedAccessAsync(
            currentUser,
            requestedIds,
            new HashSet<string> { ActionElementBrowse.ActionLetter });

        if (authorizedIds.Count == 0)
        {
            return Ok(new BatchResponseModel<ElementResponseModel>());
        }

        Guid[] orderedAuthorizedIds = requestedIds.Where(authorizedIds.Contains).ToArray();

        IEnumerable<IElement> elements = _elementService.GetByIds(orderedAuthorizedIds);
        IDictionary<Guid, IEnumerable<ContentSchedule>> schedulesByKey =
            _elementService.GetContentSchedulesByKeys(orderedAuthorizedIds);

        elements = OrderByRequestedIds(elements, orderedAuthorizedIds);

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
