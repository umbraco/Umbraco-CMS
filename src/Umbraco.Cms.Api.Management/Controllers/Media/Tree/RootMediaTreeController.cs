using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
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
public class RootMediaTreeController : MediaTreeControllerBase
{
    [ActivatorUtilitiesConstructor]
    public RootMediaTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IMediaStartNodeTreeFilterService treeFilterService,
        IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService, flagProviders, treeFilterService, mediaPresentationFactory)
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public RootMediaTreeController(
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

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MediaTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of media items from the root of the tree.")]
    [EndpointDescription("Gets a paginated collection of media items from the root of the tree with optional filtering.")]
    public async Task<ActionResult<PagedViewModel<MediaTreeItemResponseModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100,
        Guid? dataTypeId = null)
    {
        IgnoreUserStartNodesForDataType(dataTypeId);
        return await GetRoot(skip, take);
    }
}
