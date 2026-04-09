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
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Tree;

/// <summary>
/// Controller responsible for handling operations related to the ancestors of documents in the document tree.
/// </summary>
[ApiVersion("1.0")]
public class AncestorsDocumentTreeController : DocumentTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Document.Tree.AncestorsDocumentTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the Umbraco CMS.</param>
    /// <param name="userStartNodeEntitiesService">Service for resolving user start nodes for entities.</param>
    /// <param name="dataTypeService">Service for managing data types in the CMS.</param>
    /// <param name="publicAccessService">Service for handling public access permissions on content.</param>
    /// <param name="appCaches">Provides access to application-level caches.</param>
    /// <param name="backofficeSecurityAccessor">Accessor for backoffice security context and operations.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public AncestorsDocumentTreeController(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        IPublicAccessService publicAccessService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IDocumentPresentationFactory documentPresentationFactory)
        : base(
            entityService,
            userStartNodeEntitiesService,
            dataTypeService,
            publicAccessService,
            appCaches,
            backofficeSecurityAccessor,
            documentPresentationFactory)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsDocumentTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities within Umbraco.</param>
    /// <param name="flagProviders">A collection of providers for handling entity flags.</param>
    /// <param name="userStartNodeEntitiesService">Service for resolving user start node entities.</param>
    /// <param name="dataTypeService">Service for managing data types in Umbraco.</param>
    /// <param name="publicAccessService">Service for handling public access permissions.</param>
    /// <param name="appCaches">Provides access to application-level caches.</param>
    /// <param name="backofficeSecurityAccessor">Accessor for backoffice security context and operations.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public AncestorsDocumentTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        IPublicAccessService publicAccessService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IDocumentPresentationFactory documentPresentationFactory)
        : base(
            entityService,
            flagProviders,
            userStartNodeEntitiesService,
            dataTypeService,
            publicAccessService,
            appCaches,
            backofficeSecurityAccessor,
            documentPresentationFactory)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Document.Tree.AncestorsDocumentTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for document tree nodes.</param>
    /// <param name="userStartNodeEntitiesService">Service for resolving user-specific start nodes for entities.</param>
    /// <param name="dataTypeService">Service for accessing and managing data types.</param>
    /// <param name="publicAccessService">Service for handling public access permissions on documents.</param>
    /// <param name="appCaches">Provides application-level caching functionality.</param>
    /// <param name="backofficeSecurityAccessor">Accessor for backoffice security context and authentication.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    /// <param name="documentPermissionFilterService">Service for filtering documents based on user permissions.</param>
    [ActivatorUtilitiesConstructor]
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
        : base(
            entityService,
            flagProviders,
            userStartNodeEntitiesService,
            dataTypeService,
            publicAccessService,
            appCaches,
            backofficeSecurityAccessor,
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
