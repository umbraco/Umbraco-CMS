using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Tree;

/// <summary>
/// API controller responsible for retrieving multiple template tree items by key.
/// </summary>
[ApiVersion("1.0")]
public class BatchTemplateTreeController : TemplateTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchTemplateTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for template tree nodes.</param>
    public BatchTemplateTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders)
        : base(
            entityService,
            flagProviders)
    {
    }

    /// <summary>
    /// Gets the template tree items identified by the provided Ids.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="ids">The Ids of the template tree items to retrieve.</param>
    /// <returns>The template tree items, in the requested order.</returns>
    [HttpGet("batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets multiple template tree items.")]
    [EndpointDescription("Gets the template tree items identified by the provided Ids.")]
    public async Task<ActionResult<BatchResponseModel<NamedEntityTreeItemResponseModel>>> Batch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
        => await GetBatch([.. ids]);
}
