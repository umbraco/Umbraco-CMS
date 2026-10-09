using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Api.Management.Services.Flags;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Tree;

/// <summary>
/// API controller responsible for handling operations related to the root of the data type tree in Umbraco.
/// </summary>
[ApiVersion("1.0")]
public class RootDataTypeTreeController : DataTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootDataTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for tree nodes.</param>
    /// <param name="entitySearchService">Service for searching entities.</param>
    /// <param name="idKeyMap">Maps between integer identifiers and keys.</param>
    /// <param name="dataTypeService">Service for managing data types.</param>
    public RootDataTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IEntitySearchService entitySearchService, IIdKeyMap idKeyMap, IDataTypeService dataTypeService)
    : base(entityService, flagProviders, entitySearchService, idKeyMap, dataTypeService)
    {
    }

    /// <summary>
    /// Retrieves a paginated list of data type items from the root of the tree, with optional folder-only filtering.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="foldersOnly">If true, only folders are included in the results.</param>
    /// <returns>A paged view model containing data type tree item response models.</returns>
    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DataTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of data type items from the root of the tree.")]
    [EndpointDescription("Gets a paginated collection of data type items from the root of the tree with optional filtering.")]
    public async Task<ActionResult<PagedViewModel<DataTypeTreeItemResponseModel>>> Root(CancellationToken cancellationToken, int skip = 0, int take = 100, bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetRoot(skip, take);
    }
}
