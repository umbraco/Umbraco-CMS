using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Tree;

/// <summary>
/// Provides API endpoints for managing sibling data types in the Umbraco CMS tree.
/// </summary>
public class SiblingsDataTypeTreeController : DataTypeTreeControllerBase
{
    public SiblingsDataTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IDataTypeService dataTypeService)
        : base(entityService, flagProviders, dataTypeService)
    {
    }

    /// <summary>
    /// Gets a paged collection of data type tree items that are siblings of the specified data type identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="target">The unique identifier of the data type whose siblings are to be retrieved.</param>
    /// <param name="before">The number of sibling items to retrieve before the target item.</param>
    /// <param name="after">The number of sibling items to retrieve after the target item.</param>
    /// <param name="foldersOnly">If set to <c>true</c>, only folders will be included in the results; otherwise, both folders and data types are returned.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a <see cref="SubsetViewModel{T}"/> of <see cref="DataTypeTreeItemResponseModel"/> representing the sibling items.</returns>
    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<DataTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of data type tree sibling items.")]
    [EndpointDescription("Gets a paged collection of data type tree items that are siblings of the provided Id. The collection can be optionally filtered to return only folder, or folders and data types.")]
    public async Task<ActionResult<SubsetViewModel<DataTypeTreeItemResponseModel>>> Siblings(CancellationToken cancellationToken, Guid target, int before, int after, bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetSiblings(target, before, after);
    }
}
