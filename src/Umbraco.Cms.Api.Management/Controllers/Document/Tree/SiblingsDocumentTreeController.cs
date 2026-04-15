using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
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
/// API controller responsible for managing and retrieving sibling documents within the content tree.
/// </summary>
[ApiVersion("1.0")]
public class SiblingsDocumentTreeController : DocumentTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsDocumentTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for document tree nodes.</param>
    /// <param name="treeFilterService">Service for filtering document tree entities based on user start nodes.</param>
    /// <param name="publicAccessService">Service for handling public access permissions on documents.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    /// <param name="documentPermissionFilterService">Service for filtering documents based on user permissions.</param>
    [ActivatorUtilitiesConstructor]
    public SiblingsDocumentTreeController(
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

    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsDocumentTreeController"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor exists solely to disambiguate DI container constructor resolution between the new
    /// and the existing obsolete constructors; all parameters except those forwarded to the non-obsolete
    /// constructor are ignored.
    /// </remarks>
    /// <param name="entityService">Service for managing and retrieving entities within Umbraco.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for document tree nodes.</param>
    /// <param name="userStartNodeEntitiesService">Service for resolving user start nodes for entities.</param>
    /// <param name="dataTypeService">Service for accessing and managing data types.</param>
    /// <param name="treeFilterService">Service for filtering document tree entities based on user start nodes.</param>
    /// <param name="publicAccessService">Service for handling public access permissions on documents.</param>
    /// <param name="appCaches">Provides access to application-level caches.</param>
    /// <param name="backofficeSecurityAccessor">Accessor for backoffice security context and operations.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    /// <param name="documentPermissionFilterService">Service for filtering documents based on user permissions.</param>
    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public SiblingsDocumentTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        IDocumentStartNodeTreeFilterService treeFilterService,
        IPublicAccessService publicAccessService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IDocumentPresentationFactory documentPresentationFactory,
        IDocumentPermissionFilterService documentPermissionFilterService)
        : this(entityService, flagProviders, treeFilterService, publicAccessService, documentPresentationFactory, documentPermissionFilterService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsDocumentTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    /// <param name="userStartNodeEntitiesService">Service for resolving user start nodes for entities.</param>
    /// <param name="dataTypeService">Service for accessing and managing data types.</param>
    /// <param name="publicAccessService">Service for handling public access permissions.</param>
    /// <param name="appCaches">Provides application-level caching functionality.</param>
    /// <param name="backofficeSecurityAccessor">Accessor for backoffice security context and operations.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public SiblingsDocumentTreeController(
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
    /// Initializes a new instance of the <see cref="SiblingsDocumentTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the Umbraco CMS.</param>
    /// <param name="flagProviders">A collection of providers that supply flag information for entities.</param>
    /// <param name="userStartNodeEntitiesService">Service for resolving user start node entities.</param>
    /// <param name="dataTypeService">Service for managing data types in the CMS.</param>
    /// <param name="publicAccessService">Service for handling public access permissions.</param>
    /// <param name="appCaches">Provides access to application-level caches.</param>
    /// <param name="backofficeSecurityAccessor">Accessor for backoffice security context and operations.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    [Obsolete("Please use the constructor accepting IDocumentStartNodeTreeFilterService. Scheduled for removal in Umbraco 19.")]
    public SiblingsDocumentTreeController(
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
    /// Initializes a new instance of the <see cref="SiblingsDocumentTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities within Umbraco.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for document tree nodes.</param>
    /// <param name="userStartNodeEntitiesService">Service for resolving user-specific start nodes for entities.</param>
    /// <param name="dataTypeService">Service for accessing and managing data types.</param>
    /// <param name="publicAccessService">Service for handling public access permissions on documents.</param>
    /// <param name="appCaches">Provides application-level caching functionality.</param>
    /// <param name="backofficeSecurityAccessor">Accessor for backoffice security context and user information.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    /// <param name="documentPermissionFilterService">Service for filtering documents based on user permissions.</param>
    [Obsolete("Please use the constructor accepting IDocumentStartNodeTreeFilterService. Scheduled for removal in Umbraco 19.")]
    public SiblingsDocumentTreeController(
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

    /// <summary>
    /// Retrieves a subset of sibling document tree items for a specified document.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="target">The unique identifier of the document whose siblings are to be retrieved.</param>
    /// <param name="before">The number of sibling items to include before the target document in the result set.</param>
    /// <param name="after">The number of sibling items to include after the target document in the result set.</param>
    /// <param name="dataTypeId">An optional data type identifier to filter the sibling items. If null, no filtering is applied.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="ActionResult{T}"/> with a <see cref="SubsetViewModel{DocumentTreeItemResponseModel}"/> representing the sibling document tree items.</returns>
    [HttpGet("siblings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(SubsetViewModel<DocumentTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of document tree sibling items.")]
    [EndpointDescription("Gets a collection of document tree items that are siblings of the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<DocumentTreeItemResponseModel>>> Siblings(CancellationToken cancellationToken, Guid target, int before, int after, Guid? dataTypeId = null)
    {
        IgnoreUserStartNodesForDataType(dataTypeId);
        return await GetSiblings(target, before, after);
    }
}
