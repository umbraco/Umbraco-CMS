using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.Services.PermissionFilter;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Tree;

/// <summary>
/// Serves as the base controller for element tree operations within the Umbraco CMS Management API.
/// Provides shared functionality for derived element tree controllers including user start node support and permission filtering.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Element}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Element))]
[Authorize(Policy = AuthorizationPolicies.SectionAccessForElementTree)]
public class ElementTreeControllerBase : UserStartNodeFolderTreeControllerBase<ElementTreeItemResponseModel>
{
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IElementPresentationFactory _elementPresentationFactory;
    private readonly IElementPermissionFilterService _elementPermissionFilterService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementTreeControllerBase"/> class.
    /// </summary>
    /// <param name="entityService">Service for retrieving entity data.</param>
    /// <param name="flagProviders">Collection of flag providers for tree item flags.</param>
    /// <param name="userStartNodeEntitiesService">Service for user start node entity resolution.</param>
    /// <param name="dataTypeService">Service for data type operations.</param>
    /// <param name="appCaches">Application caches for performance optimization.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    /// <param name="elementPermissionFilterService">Service for filtering tree entities based on element permissions.</param>
    public ElementTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IElementPresentationFactory elementPresentationFactory,
        IElementPermissionFilterService elementPermissionFilterService)
        : base(entityService, flagProviders, userStartNodeEntitiesService, dataTypeService)
    {
        _appCaches = appCaches;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _elementPresentationFactory = elementPresentationFactory;
        _elementPermissionFilterService = elementPermissionFilterService;
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Element;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.ElementContainer;

    protected override int[] GetUserStartNodeIds()
        => _backOfficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .CalculateElementStartNodeIds(EntityService, _appCaches)
           ?? [];

    protected override string[] GetUserStartNodePaths()
        => _backOfficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .GetElementStartNodePaths(EntityService, _appCaches)
           ?? [];

    protected override ElementTreeItemResponseModel MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        ElementTreeItemResponseModel responseModel = base.MapTreeItemViewModel(parentKey, entity);

        if (entity is IElementEntitySlim elementEntitySlim)
        {
            responseModel.HasChildren = elementEntitySlim.HasChildren;
            responseModel.CreateDate = elementEntitySlim.CreateDate;
            responseModel.DocumentType = _elementPresentationFactory.CreateDocumentTypeReferenceResponseModel(elementEntitySlim);
            responseModel.Variants = _elementPresentationFactory.CreateVariantsItemResponseModels(elementEntitySlim);
        }

        return responseModel;
    }

    protected override ElementTreeItemResponseModel MapTreeItemViewModelAsNoAccess(Guid? parentKey, IEntitySlim entity)
    {
        ElementTreeItemResponseModel viewModel = MapTreeItemViewModel(parentKey, entity);
        viewModel.NoAccess = true;
        return viewModel;
    }

    /// <inheritdoc/>
    protected override Task<(IEntitySlim[] Entities, long TotalItems)> FilterTreeEntities(
        IEntitySlim[] entities,
        long totalItems)
        => _elementPermissionFilterService.FilterAsync(entities, totalItems);

    /// <inheritdoc/>
    protected override Task<(IEntitySlim[] Entities, long TotalBefore, long TotalAfter)> FilterTreeEntities(
        Guid targetKey,
        IEntitySlim[] entities,
        long totalBefore,
        long totalAfter)
        => _elementPermissionFilterService.FilterAsync(targetKey, entities, totalBefore, totalAfter);
}
