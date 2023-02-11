using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Controllers.Tree;
using Umbraco.Cms.ManagementApi.Services.Entities;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Media.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Media}/tree")]
[OpenApiTag(nameof(Constants.UdiEntityType.Media))]
public class MediaTreeControllerBase : UserStartNodeTreeControllerBase<ContentTreeItemViewModel>
{
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

    public MediaTreeControllerBase(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        : base(entityService, userStartNodeEntitiesService, dataTypeService)
    {
        _appCaches = appCaches;
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Media;

    protected override Ordering ItemOrdering => Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.SortOrder));

    protected override ContentTreeItemViewModel MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        ContentTreeItemViewModel viewModel = base.MapTreeItemViewModel(parentKey, entity);

        if (entity is IMediaEntitySlim mediaEntitySlim)
        {
            viewModel.Icon = mediaEntitySlim.ContentTypeIcon ?? viewModel.Icon;
        }

        return viewModel;
    }

    // TODO: delete these (faking start node setup for unlimited editor)
    protected override int[] GetUserStartNodeIds() => new[] { -1 };

    protected override string[] GetUserStartNodePaths() => Array.Empty<string>();

    // TODO: use these implementations instead of the dummy ones above once we have backoffice auth in place
    // protected override int[] GetUserStartNodeIds()
    //     => _backofficeSecurityAccessor
    //            .BackOfficeSecurity?
    //            .CurrentUser?
    //            .CalculateMediaStartNodeIds(EntityService, _appCaches)
    //        ?? Array.Empty<int>();
    //
    // protected override string[] GetUserStartNodePaths()
    //     => _backofficeSecurityAccessor
    //            .BackOfficeSecurity?
    //            .CurrentUser?
    //            .GetMediaStartNodePaths(EntityService, _appCaches)
    //        ?? Array.Empty<string>();
}
