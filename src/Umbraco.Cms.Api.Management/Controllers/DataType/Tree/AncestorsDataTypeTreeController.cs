using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Tree;

/// <summary>
/// Controller responsible for handling operations related to the ancestors tree structure of data types.
/// </summary>
[ApiVersion("1.0")]
public class AncestorsDataTypeTreeController : DataTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsDataTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for tree nodes.</param>
    /// <param name="entitySearchService">Service for searching entities.</param>
    /// <param name="idKeyMap">Maps between integer identifiers and keys.</param>
    /// <param name="dataTypeService">Service for managing data types.</param>
    public AncestorsDataTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IEntitySearchService entitySearchService, IIdKeyMap idKeyMap, IDataTypeService dataTypeService)
    : base(entityService, flagProviders, entitySearchService, idKeyMap, dataTypeService)
    {
    }

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DataTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor data type folders.")]
    [EndpointDescription("Gets a collection of data type folders that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<DataTypeTreeItemResponseModel>>> Ancestors(CancellationToken cancellationToken, Guid descendantId)
        => await GetAncestors(descendantId);
}
