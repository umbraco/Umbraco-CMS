using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.Services.PermissionFilter;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Tree;

[ApiVersion("1.0")]
public class AncestorsDocumentTreeController : DocumentTreeControllerBase
{
    [ActivatorUtilitiesConstructor]
    public AncestorsDocumentTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IDocumentStartNodeTreeFilterService treeFilterService,
        IPublicAccessService publicAccessService,
        IDocumentPresentationFactory documentPresentationFactory,
        IDocumentPermissionFilterService documentPermissionFilterService)
        : base(
            entityService,
            flagProviders,
            treeFilterService,
            publicAccessService,
            documentPresentationFactory,
            documentPermissionFilterService)
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public AncestorsDocumentTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        IPublicAccessService publicAccessService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IDocumentPresentationFactory documentPresentationFactory)
        : this(
            entityService,
            flagProviders,
            StaticServiceProvider.Instance.GetRequiredService<IDocumentStartNodeTreeFilterService>(),
            publicAccessService,
            documentPresentationFactory,
            StaticServiceProvider.Instance.GetRequiredService<IDocumentPermissionFilterService>())
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public AncestorsDocumentTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        IPublicAccessService publicAccessService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IDocumentPresentationFactory documentPresentationFactory,
        IDocumentPermissionFilterService documentPermissionFilterService)
        : this(
            entityService,
            flagProviders,
            StaticServiceProvider.Instance.GetRequiredService<IDocumentStartNodeTreeFilterService>(),
            publicAccessService,
            documentPresentationFactory,
            documentPermissionFilterService)
    {
    }

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor document items.")]
    [EndpointDescription("Gets a collection of document items that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<DocumentTreeItemResponseModel>>> Ancestors(CancellationToken cancellationToken, Guid descendantId)
        => await GetAncestors(descendantId);
}
