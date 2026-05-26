using System.ComponentModel;
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
/// Controller responsible for handling operations related to the child nodes of documents within the document tree.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenDocumentTreeController : DocumentTreeControllerBase
{
    /// <param name="flagProviders">A collection of providers that supply flags for document tree nodes.</param>
    /// <param name="treeFilterService">Service for filtering document tree entities based on user start nodes.</param>
    /// <param name="publicAccessService">Service for handling public access permissions on documents.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    /// <param name="documentPermissionFilterService">Service for filtering documents based on user permissions.</param>
    [ActivatorUtilitiesConstructor]
    public ChildrenDocumentTreeController(
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
    /// Initializes a new instance of the <see cref="ChildrenDocumentTreeController"/> class.
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
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ChildrenDocumentTreeController(
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
    /// Initializes a new instance of the <see cref="ChildrenDocumentTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    [Obsolete("Please use the constructor accepting IDocumentStartNodeTreeFilterService. Scheduled for removal in Umbraco 19.")]
    public ChildrenDocumentTreeController(
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
    /// Initializes a new instance of the <see cref="ChildrenDocumentTreeController"/> class, which manages operations related to the children of a document tree node in the Umbraco CMS Management API.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities.</param>
    /// <param name="flagProviders">A collection of providers for entity flags.</param>
    /// <param name="userStartNodeEntitiesService">Service for resolving user start nodes.</param>
    /// <param name="dataTypeService">Service for managing data types.</param>
    /// <param name="publicAccessService">Service for handling public access permissions.</param>
    /// <param name="appCaches">Provides access to application-level caches.</param>
    /// <param name="backofficeSecurityAccessor">Accessor for backoffice security context.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    /// <param name="documentPermissionFilterService">Service for filtering documents based on permissions.</param>
    [Obsolete("Please use the constructor accepting IDocumentStartNodeTreeFilterService. Scheduled for removal in Umbraco 19.")]
    public ChildrenDocumentTreeController(
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
    /// Retrieves a paginated list of document tree items that are direct children of the specified parent document node.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="parentId">The unique identifier of the parent document node.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return (used for pagination).</param>
    /// <param name="dataTypeId">An optional data type identifier to filter the child items.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a <see cref="PagedViewModel{T}"/> of <see cref="DocumentTreeItemResponseModel"/> representing the child document tree items.</returns>
    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of document tree child items.")]
    [EndpointDescription("Gets a paginated collection of document tree items that are children of the provided parent Id.")]
    public async Task<ActionResult<PagedViewModel<DocumentTreeItemResponseModel>>> Children(
        CancellationToken cancellationToken,
        Guid parentId,
        int skip = 0,
        int take = 100,
        Guid? dataTypeId = null)
    {
        IgnoreUserStartNodesForDataType(dataTypeId);
        return await GetChildren(parentId, skip, take);
    }
}
