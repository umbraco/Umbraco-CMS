using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Tree;

[ApiVersion("1.0")]
public class AncestorsMediaTreeController : MediaTreeControllerBase
{
    [ActivatorUtilitiesConstructor]
    public AncestorsMediaTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IMediaStartNodeTreeFilterService treeFilterService,
        IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService, flagProviders, treeFilterService, mediaPresentationFactory)
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public AncestorsMediaTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IMediaPresentationFactory mediaPresentationFactory)
        : this(
            entityService,
            flagProviders,
            StaticServiceProvider.Instance.GetRequiredService<IMediaStartNodeTreeFilterService>(),
            mediaPresentationFactory)
    {
    }

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MediaTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor media items.")]
    [EndpointDescription("Gets a collection of media items that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<MediaTreeItemResponseModel>>> Ancestors(
        CancellationToken cancellationToken,
        Guid descendantId)
        => await GetAncestors(descendantId);
}
