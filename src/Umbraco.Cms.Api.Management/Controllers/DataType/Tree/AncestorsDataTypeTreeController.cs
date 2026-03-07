using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
    /// Initializes a new instance of the <see cref="AncestorsDataTypeTreeController"/> class, which provides API endpoints for retrieving ancestor data types in the tree structure.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the API.</param>
    /// <param name="dataTypeService">Service used for data type management and retrieval.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public AncestorsDataTypeTreeController(IEntityService entityService, IDataTypeService dataTypeService)
        : base(entityService, dataTypeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsDataTypeTreeController"/> class, which manages operations related to ancestor data type trees in the Umbraco CMS.
    /// </summary>
    /// <param name="entityService">Service used for entity-related operations.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for tree nodes.</param>
    /// <param name="dataTypeService">Service used for data type management operations.</param>
    [ActivatorUtilitiesConstructor]
    public AncestorsDataTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IDataTypeService dataTypeService)
    : base(entityService, flagProviders, dataTypeService)
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
