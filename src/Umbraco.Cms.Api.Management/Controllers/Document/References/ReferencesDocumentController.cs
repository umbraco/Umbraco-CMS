using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document.References;

[ApiVersion("1.0")]
public class ReferencesDocumentController : DocumentControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ReferencesDocumentController(ITrackedReferencesService trackedReferencesService, IUmbracoMapper umbracoMapper)
    {
        _trackedReferencesService = trackedReferencesService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a page list of tracked references for the current item, so you can see where an item is being used.
    /// </summary>
    /// <remarks>
    ///     Used by info tabs on content, media etc. and for the delete and unpublish of single items.
    ///     This is basically finding parents of relations.
    /// </remarks>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentReferenceResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<DocumentReferenceResponseModel>>> Get(
        Guid id,
        int skip = 0,
        int take = 20)
    {
        PagedModel<RelationItemModel> relationItems = await _trackedReferencesService.GetPagedRelationsForItemAsync(id, skip, take, false);

        var pagedViewModel = new PagedViewModel<DocumentReferenceResponseModel>
        {
            Total = relationItems.Total,
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, DocumentReferenceResponseModel>(relationItems.Items),
        };

        return await Task.FromResult(pagedViewModel);
    }
}
