using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.Services.PermissionFilter;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Tree;

/// <summary>
/// Serves as the base controller for document tree management in the Umbraco CMS API, providing shared functionality for document tree operations.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Document}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
[Authorize(Policy = AuthorizationPolicies.SectionAccessForContentTree)]
public abstract class DocumentTreeControllerBase : UserStartNodeTreeControllerBase<DocumentTreeItemResponseModel>
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;
    private readonly IDocumentPermissionFilterService _documentPermissionFilterService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentTreeControllerBase"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for document tree nodes.</param>
    /// <param name="treeFilterService">Service for filtering document tree entities based on user start nodes.</param>
    /// <param name="publicAccessService">Service for handling public access permissions on documents.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    /// <param name="documentPermissionFilterService">Service for filtering documents based on user permissions.</param>
    [ActivatorUtilitiesConstructor]
    protected DocumentTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IDocumentStartNodeTreeFilterService treeFilterService,
        IPublicAccessService publicAccessService,
        IDocumentPresentationFactory documentPresentationFactory,
        IDocumentPermissionFilterService documentPermissionFilterService)
        : base(entityService, flagProviders, treeFilterService)
    {
        _publicAccessService = publicAccessService;
        _documentPresentationFactory = documentPresentationFactory;
        _documentPermissionFilterService = documentPermissionFilterService;
    }

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    protected DocumentTreeControllerBase(
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
    protected DocumentTreeControllerBase(
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

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Document;

    protected override Ordering ItemOrdering => Ordering.By(Infrastructure.Persistence.Dtos.NodeDto.SortOrderColumnName);

    protected override DocumentTreeItemResponseModel MapTreeItemViewModel(Guid? parentId, IEntitySlim entity)
    {
        DocumentTreeItemResponseModel responseModel = base.MapTreeItemViewModel(parentId, entity);

        if (entity is IDocumentEntitySlim documentEntitySlim)
        {
            responseModel.IsProtected = _publicAccessService.IsProtected(entity.Path);
            responseModel.Ancestors = EntityService.GetPathKeys(entity, omitSelf: true)
                .Select(x => new ReferenceByIdModel(x));
            responseModel.IsTrashed = entity.Trashed;
            responseModel.Id = entity.Key;
            responseModel.CreateDate = entity.CreateDate;

            responseModel.Variants = _documentPresentationFactory.CreateVariantsItemResponseModels(documentEntitySlim);
            responseModel.DocumentType = _documentPresentationFactory.CreateDocumentTypeReferenceResponseModel(documentEntitySlim);
        }

        return responseModel;
    }

    /// <inheritdoc/>
    protected override Task<(IEntitySlim[] Entities, long TotalItems)> FilterTreeEntities(IEntitySlim[] entities, long totalItems)
        => _documentPermissionFilterService.FilterAsync(entities, totalItems);

    /// <inheritdoc/>
    protected override Task<(IEntitySlim[] Entities, long TotalBefore, long TotalAfter)> FilterTreeEntities(Guid targetKey, IEntitySlim[] entities, long totalBefore, long totalAfter)
        => _documentPermissionFilterService.FilterAsync(targetKey, entities, totalBefore, totalAfter);
}
