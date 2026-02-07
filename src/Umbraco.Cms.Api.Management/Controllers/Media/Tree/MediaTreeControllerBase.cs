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
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Media}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Media))]
[Authorize(Policy = AuthorizationPolicies.SectionAccessForMediaTree)]
public class MediaTreeControllerBase : UserStartNodeTreeControllerBase<MediaTreeItemResponseModel>
{
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IMediaPresentationFactory _mediaPresentationFactory;

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public MediaTreeControllerBase(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IMediaPresentationFactory mediaPresentationFactory)
        : this(
              entityService,
              StaticServiceProvider.Instance.GetRequiredService<FlagProviderCollection>(),
              userStartNodeEntitiesService,
              dataTypeService,
              appCaches,
              backofficeSecurityAccessor,
              mediaPresentationFactory)
    {
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public MediaTreeControllerBase(
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
            StaticServiceProvider.Instance.GetRequiredService<IEntitySearchService>(),
            StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>(),
            userStartNodeEntitiesService,
            dataTypeService,
            appCaches,
            backofficeSecurityAccessor,
            mediaPresentationFactory)
    {
    }

    [ActivatorUtilitiesConstructor]
    public MediaTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IEntitySearchService entitySearchService,
        IIdKeyMap idKeyMap,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService, flagProviders, entitySearchService, idKeyMap, userStartNodeEntitiesService, dataTypeService)
    {
        _appCaches = appCaches;
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
        _mediaPresentationFactory = mediaPresentationFactory;
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Media;
    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.Media;

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
