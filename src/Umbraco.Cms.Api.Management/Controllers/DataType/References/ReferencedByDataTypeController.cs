using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.References;

    /// <summary>
    /// Controller responsible for managing and retrieving information about where specific data types are referenced within the system.
    /// </summary>
[ApiVersion("1.0")]
public class ReferencedByDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IRelationTypePresentationFactory _relationTypePresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedByDataTypeController"/> class, which handles API requests related to data types referenced by other entities.
    /// </summary>
    /// <param name="dataTypeService">Service used to manage and retrieve data type information.</param>
    /// <param name="relationTypePresentationFactory">Factory for creating presentation models for relation types.</param>
    public ReferencedByDataTypeController(IDataTypeService dataTypeService, IRelationTypePresentationFactory relationTypePresentationFactory)
    {
        _dataTypeService = dataTypeService;
        _relationTypePresentationFactory = relationTypePresentationFactory;
    }

/// <summary>
/// Gets a paged list of entities that reference the specified data type, allowing you to see where it is being used.
/// </summary>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <param name="id">The unique identifier of the data type to find references for.</param>
/// <param name="skip">The number of items to skip before starting to collect the result set (used for paging).</param>
/// <param name="take">The maximum number of items to return (used for paging).</param>
/// <returns>
/// A task representing the asynchronous operation. The result contains an <see cref="ActionResult{T}"/> with a <see cref="PagedViewModel{IReferenceResponseModel}"/> listing entities that reference the specified data type.
/// </returns>
    [HttpGet("{id:guid}/referenced-by")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IReferenceResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a paged collection of entities that are referenced by a data type.")]
    [EndpointDescription("Gets a paged collection of entities that are referenced by the data type with the provided Id, so you can see where it is being used.")]
    public async Task<ActionResult<PagedViewModel<IReferenceResponseModel>>> ReferencedBy(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        PagedModel<RelationItemModel> relationItems = await _dataTypeService.GetPagedRelationsAsync(id, skip, take);

        var pagedViewModel = new PagedViewModel<IReferenceResponseModel>
        {
            Total = relationItems.Total,
            Items = await _relationTypePresentationFactory.CreateReferenceResponseModelsAsync(relationItems.Items),
        };

        return pagedViewModel;
    }
}
