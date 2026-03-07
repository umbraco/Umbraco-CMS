using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Tree;

    /// <summary>
    /// API controller responsible for retrieving and managing ancestor nodes in the template tree structure within the Umbraco CMS.
    /// </summary>
[ApiVersion("1.0")]
public class AncestorsTemplateTreeController : TemplateTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsTemplateTreeController"/> class.
    /// </summary>
    /// <param name="entityService">The <see cref="IEntityService"/> used to perform entity operations.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public AncestorsTemplateTreeController(IEntityService entityService)
        : base(entityService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsTemplateTreeController"/> class, which handles operations related to retrieving ancestor template tree nodes.
    /// </summary>
    /// <param name="entityService">The service used to manage and retrieve entities.</param>
    /// <param name="flagProviders">A collection of providers used to supply additional flags or metadata for entities.</param>
    [ActivatorUtilitiesConstructor]
    public AncestorsTemplateTreeController(IEntityService entityService, FlagProviderCollection flagProviders)
        : base(entityService, flagProviders)
    {
    }

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor template items.")]
    [EndpointDescription("Gets a collection of template items that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<NamedEntityTreeItemResponseModel>>> Ancestors(
        CancellationToken cancellationToken,
        Guid descendantId)
        => await GetAncestors(descendantId);
}
