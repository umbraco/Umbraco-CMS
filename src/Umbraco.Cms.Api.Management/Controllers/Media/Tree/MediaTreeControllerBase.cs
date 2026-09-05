using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Tree;

/// <summary>
/// Serves as the base controller for media tree operations in the Umbraco CMS API, providing shared functionality for derived media tree controllers.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Media}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Media))]
[Authorize(Policy = AuthorizationPolicies.SectionAccessForMediaTree)]
public class MediaTreeControllerBase : UserStartNodeTreeControllerBase<MediaTreeItemResponseModel>
{
    private readonly IMediaPresentationFactory _mediaPresentationFactory;

    // Only populated by the obsolete constructor path; used solely by the obsolete
    // GetUserStartNodeIds / GetUserStartNodePaths overrides below.
    private readonly AppCaches? _appCaches;
    private readonly IBackOfficeSecurityAccessor? _backofficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTreeControllerBase"/> class.
    /// </summary>
    /// <param name="entityService">Service for accessing and managing entities within the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for entities.</param>
    /// <param name="userStartNodeEntitiesService">Service for resolving user-specific start nodes for entities.</param>
    /// <param name="dataTypeService">Service for managing data types in the application.</param>
    /// <param name="appCaches">Provides access to application-level caches.</param>
    /// <param name="backofficeSecurityAccessor">Accessor for backoffice security context and operations.</param>
    /// <param name="mediaPresentationFactory">Factory for creating media presentation models.</param>
    [Obsolete("Please use the constructor accepting IMediaStartNodeTreeFilterService. Scheduled for removal in Umbraco 19.")]
    public MediaTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IMediaPresentationFactory mediaPresentationFactory)
        : base(
              entityService,
              flagProviders,
              userStartNodeEntitiesService,
              dataTypeService)
    {
        _mediaPresentationFactory = mediaPresentationFactory;
        _appCaches = appCaches;
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTreeControllerBase"/> class.
    /// </summary>
    /// <param name="entityService">Service for accessing and managing entities within the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for entities.</param>
    /// <param name="treeFilterService">Service for filtering media tree entities based on user start nodes.</param>
    /// <param name="mediaPresentationFactory">Factory for creating media presentation models.</param>
    [ActivatorUtilitiesConstructor]
    public MediaTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IMediaStartNodeTreeFilterService treeFilterService,
        IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService, flagProviders, treeFilterService) =>
        _mediaPresentationFactory = mediaPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTreeControllerBase"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is a parameter superset of the new and existing obsolete constructors. It exists
    /// solely because <see cref="ActivatorUtilitiesConstructorAttribute"/> is not honoured by the DI
    /// <c>CallSiteFactory</c> at <c>ServiceProvider</c> <c>ValidateOnBuild</c> time, which requires an
    /// unambiguous single best-match constructor; all parameters except those forwarded to the non-obsolete
    /// constructor are ignored.
    /// </remarks>
    /// <param name="entityService">Service for accessing and managing entities within the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for entities.</param>
    /// <param name="userStartNodeEntitiesService">Service for resolving user start nodes for entities.</param>
    /// <param name="dataTypeService">Service for accessing and managing data types.</param>
    /// <param name="treeFilterService">Service for filtering media tree entities based on user start nodes.</param>
    /// <param name="appCaches">Provides access to application-level caches.</param>
    /// <param name="backofficeSecurityAccessor">Accessor for backoffice security context and operations.</param>
    /// <param name="mediaPresentationFactory">Factory for creating media presentation models.</param>
    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public MediaTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        IMediaStartNodeTreeFilterService treeFilterService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IMediaPresentationFactory mediaPresentationFactory)
        : this(entityService, flagProviders, treeFilterService, mediaPresentationFactory)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Media;

    protected override Ordering ItemOrdering => Ordering.By(Infrastructure.Persistence.Dtos.NodeDto.SortOrderColumnName);

    protected override async Task<MediaTreeItemResponseModel> MapTreeItemViewModelAsync(Guid? parentKey, IEntitySlim entity)
    {
        MediaTreeItemResponseModel responseModel = await base.MapTreeItemViewModelAsync(parentKey, entity);

        if (entity is IMediaEntitySlim mediaEntitySlim)
        {
            responseModel.IsTrashed = entity.Trashed;
            responseModel.Id = entity.Key;
            responseModel.CreateDate = entity.CreateDate;

            responseModel.Variants = _mediaPresentationFactory.CreateVariantsItemResponseModels(mediaEntitySlim);
            responseModel.MediaType = _mediaPresentationFactory.CreateMediaTypeReferenceResponseModel(mediaEntitySlim);
        }

        return responseModel;
    }

    // Only invoked via the CallbackStartNodeTreeFilterService wired up by the obsolete
    // UserStartNodeTreeControllerBase constructor. The non-obsolete constructor path
    // routes start node resolution through IMediaStartNodeTreeFilterService and
    // never calls these overrides; hence the null-forgiving operator on _appCaches.
    [Obsolete("No longer used. Register a custom IMediaStartNodeTreeFilterService instead. Scheduled for removal in Umbraco 19.")]
    protected override int[] GetUserStartNodeIds()
        => _backofficeSecurityAccessor?
               .BackOfficeSecurity?
               .CurrentUser?
               .CalculateMediaStartNodeIds(EntityService, _appCaches!)
           ?? [];

    [Obsolete("No longer used. Register a custom IMediaStartNodeTreeFilterService instead. Scheduled for removal in Umbraco 19.")]
    protected override string[] GetUserStartNodePaths()
        => _backofficeSecurityAccessor?
               .BackOfficeSecurity?
               .CurrentUser?
               .GetMediaStartNodePaths(EntityService, _appCaches!)
           ?? [];
}
