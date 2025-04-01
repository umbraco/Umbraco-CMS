using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DateType.References;

[ApiVersion("1.0")]
public class ReferencedByDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IRelationTypePresentationFactory _relationTypePresentationFactory;

    public ReferencedByDataTypeController(IDataTypeService dataTypeService, IRelationTypePresentationFactory relationTypePresentationFactory)
    {
        _dataTypeService = dataTypeService;
        _relationTypePresentationFactory = relationTypePresentationFactory;
    }

    /// <summary>
    ///     Gets a paged list of references for the current data type, so you can see where is is being used.
    /// </summary>
    [HttpGet("{id:guid}/referenced-by")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IReferenceResponseModel>), StatusCodes.Status200OK)]
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
