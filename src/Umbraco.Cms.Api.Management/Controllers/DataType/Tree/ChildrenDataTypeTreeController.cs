using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Flags;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Tree;

/// <summary>
/// Controller responsible for handling operations related to the child nodes of the data type tree in the management API.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenDataTypeTreeController : DataTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenDataTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for managing and retrieving entities within the system.</param>
    /// <param name="dataTypeService">Service used for managing and retrieving data types.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public ChildrenDataTypeTreeController(IEntityService entityService, IDataTypeService dataTypeService)
        : base(entityService, dataTypeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenDataTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for managing and retrieving entities within the system.</param>
    /// <param name="flagProviders">A collection of providers that supply additional flags or metadata for entities.</param>
    /// <param name="dataTypeService">Service responsible for operations related to data types.</param>
    [ActivatorUtilitiesConstructor]
    public ChildrenDataTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IDataTypeService dataTypeService)
        : base(entityService, flagProviders, dataTypeService)
    {
    }

    /// <summary>
    /// Retrieves a paginated collection of data type tree items that are children of the specified parent ID.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="parentId">The unique identifier of the parent data type tree item whose children are to be retrieved.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return (used for pagination).</param>
    /// <param name="foldersOnly">If set to <c>true</c>, only folder items will be included in the results.</param>
    /// <returns>A <see cref="PagedViewModel{T}"/> containing <see cref="DataTypeTreeItemResponseModel"/> instances representing the child items.</returns>
    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DataTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of data type tree child items.")]
    [EndpointDescription("Gets a paginated collection of data type tree items that are children of the provided parent Id.")]
    public async Task<ActionResult<PagedViewModel<DataTypeTreeItemResponseModel>>> Children(CancellationToken cancellationToken, Guid parentId, int skip = 0, int take = 100, bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetChildren(parentId, skip, take);
    }
}
