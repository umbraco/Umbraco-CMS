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
    /// API controller responsible for retrieving and managing child nodes within the template tree structure in Umbraco CMS.
    /// Provides endpoints for accessing child templates under a specified parent template.
    /// </summary>
[ApiVersion("1.0")]
public class ChildrenTemplateTreeController : TemplateTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenTemplateTreeController"/> class.
    /// </summary>
    /// <param name="entityService">The service used to manage and retrieve entities within the controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public ChildrenTemplateTreeController(IEntityService entityService)
        : base(entityService)
    {
    }

/// <summary>
/// Initializes a new instance of the <see cref="ChildrenTemplateTreeController"/> class, which manages the retrieval of child template tree items.
/// </summary>
/// <param name="entityService">The service used to interact with entities in the system.</param>
/// <param name="flagProviders">A collection of providers that supply additional flags or metadata for entities.</param>
    [ActivatorUtilitiesConstructor]
    public ChildrenTemplateTreeController(IEntityService entityService, FlagProviderCollection flagProviders)
        : base(entityService, flagProviders)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of template tree child items.")]
    [EndpointDescription("Gets a paginated collection of template tree items that are children of the provided parent Id.")]
    public async Task<ActionResult<PagedViewModel<NamedEntityTreeItemResponseModel>>> Children(
        CancellationToken cancellationToken,
        Guid parentId,
        int skip = 0,
        int take = 100)
        => await GetChildren(parentId, skip, take);
}
