using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Tree;

/// <summary>
/// API controller responsible for retrieving multiple member type tree items by key.
/// </summary>
[ApiVersion("1.0")]
public class BatchMemberTypeTreeController : MemberTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchMemberTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for member type tree nodes.</param>
    /// <param name="entitySearchService">Service for searching entities in the tree.</param>
    /// <param name="idKeyMap">Map between entity IDs and keys.</param>
    /// <param name="memberTypeService">Service for managing member types.</param>
    public BatchMemberTypeTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IEntitySearchService entitySearchService,
        IIdKeyMap idKeyMap,
        IMemberTypeService memberTypeService)
        : base(
            entityService,
            flagProviders,
            entitySearchService,
            idKeyMap,
            memberTypeService)
    {
    }

    /// <summary>
    /// Gets the member type tree items identified by the provided Ids.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="ids">The Ids of the member type tree items to retrieve.</param>
    /// <returns>The member type tree items, in the requested order.</returns>
    [HttpGet("batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<MemberTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets multiple member type tree items.")]
    [EndpointDescription("Gets the member type tree items identified by the provided Ids.")]
    public async Task<ActionResult<BatchResponseModel<MemberTypeTreeItemResponseModel>>> Batch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
        => await GetBatch([.. ids]);
}
