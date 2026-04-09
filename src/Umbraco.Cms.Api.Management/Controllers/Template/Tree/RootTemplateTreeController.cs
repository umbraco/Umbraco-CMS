using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Tree;

/// <summary>
/// Serves as the root controller for managing the template tree structure in the Umbraco CMS Management API.
/// Provides endpoints for retrieving and interacting with template nodes at the root level of the tree.
/// </summary>
[ApiVersion("1.0")]
public class RootTemplateTreeController : TemplateTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootTemplateTreeController"/> class.
    /// </summary>
    /// <param name="entityService">An <see cref="IEntityService"/> instance used for managing entities within the controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public RootTemplateTreeController(IEntityService entityService)
        : base(entityService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootTemplateTreeController"/> class.
    /// </summary>
    /// <param name="entityService">The <see cref="IEntityService"/> used for managing template entities.</param>
    /// <param name="flagProviders">The collection of <see cref="FlagProviderCollection"/> used to provide flag information for templates.</param>
    [ActivatorUtilitiesConstructor]
    public RootTemplateTreeController(IEntityService entityService, FlagProviderCollection flagProviders)
        : base(entityService, flagProviders)
    {
    }

    /// <summary>
    /// Retrieves a paginated collection of template items from the root of the template tree.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip for pagination. Defaults to 0.</param>
    /// <param name="take">The maximum number of items to return for pagination. Defaults to 100.</param>
    /// <returns>
    /// A <see cref="PagedViewModel{NamedEntityTreeItemResponseModel}"/> containing the template items at the root of the tree.
    /// </returns>
    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of template items from the root of the tree.")]
    [EndpointDescription("Gets a paginated collection of template items from the root of the tree with optional filtering.")]
    public async Task<ActionResult<PagedViewModel<NamedEntityTreeItemResponseModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
        => await GetRoot(skip, take);
}
