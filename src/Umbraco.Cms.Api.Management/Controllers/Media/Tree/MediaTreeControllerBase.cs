using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IMediaPresentationFactory _mediaPresentationFactory;

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
    public MediaTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService, flagProviders, userStartNodeEntitiesService, dataTypeService)
    {
        _appCaches = appCaches;
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
        _mediaPresentationFactory = mediaPresentationFactory;
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Media;

    protected override Ordering ItemOrdering => Ordering.By(Infrastructure.Persistence.Dtos.NodeDto.SortOrderColumnName);

    protected override MediaTreeItemResponseModel MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        MediaTreeItemResponseModel responseModel = base.MapTreeItemViewModel(parentKey, entity);

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

    protected override int[] GetUserStartNodeIds()
        => _backofficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .CalculateMediaStartNodeIds(EntityService, _appCaches)
           ?? Array.Empty<int>();

    protected override string[] GetUserStartNodePaths()
        => _backofficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .GetMediaStartNodePaths(EntityService, _appCaches)
           ?? Array.Empty<string>();
}
