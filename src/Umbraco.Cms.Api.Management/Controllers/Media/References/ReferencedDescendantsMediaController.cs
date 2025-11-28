using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.References;

[ApiVersion("1.0")]
public class ReferencedDescendantsMediaController : MediaControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesSkipTakeService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IEntityService _entityService;

    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public ReferencedDescendantsMediaController(
        ITrackedReferencesService trackedReferencesSkipTakeService,
        IUmbracoMapper umbracoMapper)
        : this(
              trackedReferencesSkipTakeService,
              umbracoMapper,
              StaticServiceProvider.Instance.GetRequiredService<IEntityService>())
    {
    }

    [ActivatorUtilitiesConstructor]
    public ReferencedDescendantsMediaController(
        ITrackedReferencesService trackedReferencesSkipTakeService,
        IUmbracoMapper umbracoMapper,
        IEntityService entityService)
    {
        _trackedReferencesSkipTakeService = trackedReferencesSkipTakeService;
        _umbracoMapper = umbracoMapper;
        _entityService = entityService;
    }

    /// <summary>
    ///     Gets a page list of the child nodes of the current item used in any kind of relation.
    /// </summary>
    /// <remarks>
    ///     Used when deleting and unpublishing a single item to check if this item has any descending items that are in any
    ///     kind of relation.
    ///     This is basically finding the descending items which are children in relations.
    /// </remarks>
    [HttpGet("{id:guid}/referenced-descendants")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ReferenceByIdModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedViewModel<ReferenceByIdModel>>> ReferencedDescendants(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        IEntitySlim? entity = _entityService.Get(id, UmbracoObjectTypes.Media);
        if (entity is null)
        {
            return NotFound();
        }

        PagedModel<RelationItemModel> relationItems = await _trackedReferencesSkipTakeService.GetPagedDescendantsInReferencesAsync(id, skip, take, true);
        var pagedViewModel = new PagedViewModel<ReferenceByIdModel>
        {
            Total = relationItems.Total,
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, ReferenceByIdModel>(relationItems.Items),
        };

        return pagedViewModel;
    }
}
