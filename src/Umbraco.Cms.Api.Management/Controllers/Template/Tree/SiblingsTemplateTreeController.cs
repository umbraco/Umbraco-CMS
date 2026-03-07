using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Tree;

    /// <summary>
    /// API controller for retrieving and managing sibling templates within the template tree structure in Umbraco CMS.
    /// Provides endpoints related to the organization and navigation of template siblings.
    /// </summary>
public class SiblingsTemplateTreeController : TemplateTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsTemplateTreeController"/> class.
    /// </summary>
    /// <param name="entityService">The service used to manage entities within the controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public SiblingsTemplateTreeController(IEntityService entityService)
        : base(entityService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsTemplateTreeController"/> class, responsible for handling sibling template tree operations.
    /// </summary>
    /// <param name="entityService">The service used to manage and retrieve entities.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for entities.</param>
    [ActivatorUtilitiesConstructor]
    public SiblingsTemplateTreeController(IEntityService entityService, FlagProviderCollection flagProviders)
        : base(entityService, flagProviders)
    {
    }


    /// <summary>
    /// Retrieves a subset of template tree items that are siblings of the specified template.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="target">The unique identifier of the template whose siblings are to be retrieved.</param>
    /// <param name="before">The number of sibling items to include before the target template in the result set.</param>
    /// <param name="after">The number of sibling items to include after the target template in the result set.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a <see cref="SubsetViewModel{T}"/> of <see cref="NamedEntityTreeItemResponseModel"/> representing the sibling template tree items.
    /// </returns>
    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of template tree sibling items.")]
    [EndpointDescription("Gets a collection of template tree items that are siblings of the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<NamedEntityTreeItemResponseModel>>> Siblings(
        CancellationToken cancellationToken,
        Guid target,
        int before,
        int after) =>
        await GetSiblings(target, before, after);
}
