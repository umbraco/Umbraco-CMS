using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Api.Management.Services.Flags;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup.Tree;

    /// <summary>
    /// Controller responsible for handling operations related to the root of the member group tree in the management API.
    /// </summary>
[ApiVersion("1.0")]
public class RootMemberGroupTreeController : MemberGroupTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootMemberGroupTreeController"/> class.
    /// </summary>
    /// <param name="entityService">The <see cref="IEntityService"/> instance used to perform member group operations.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public RootMemberGroupTreeController(IEntityService entityService)
        : base(entityService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootMemberGroupTreeController"/> class.
    /// </summary>
    /// <param name="entityService">The <see cref="IEntityService"/> used to manage member group entities.</param>
    /// <param name="flagProviders">A collection of <see cref="FlagProviderCollection"/> used to provide flagging functionality.</param>
    [ActivatorUtilitiesConstructor]
    public RootMemberGroupTreeController(IEntityService entityService, FlagProviderCollection flagProviders)
        : base(entityService, flagProviders)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of member group items from the root of the tree.")]
    [EndpointDescription("Gets a paginated collection of member group items from the root of the tree with optional filtering.")]
    public async Task<ActionResult<PagedViewModel<NamedEntityTreeItemResponseModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
        => await GetRoot(skip, take);
}
