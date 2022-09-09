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

namespace Umbraco.Cms.ManagementApi.Controllers.Document.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Document}/tree")]
[OpenApiTag(nameof(Constants.UdiEntityType.Document))]
public abstract class DocumentTreeControllerBase : UserStartNodeTreeControllerBase<DocumentTreeItemViewModel>
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

    protected DocumentTreeControllerBase(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IPublicAccessService publicAccessService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        : base(entityService, userStartNodeEntitiesService)
    {
        _publicAccessService = publicAccessService;
        _appCaches = appCaches;
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Document;

    protected override DocumentTreeItemViewModel MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        DocumentTreeItemViewModel viewModel = base.MapTreeItemViewModel(parentKey, entity);

        if (entity is IDocumentEntitySlim documentEntitySlim)
        {
            viewModel.IsPublished = documentEntitySlim.Published;
            viewModel.IsEdited = documentEntitySlim.Edited;
            viewModel.Icon = documentEntitySlim.ContentTypeIcon ?? viewModel.Icon;
            viewModel.IsProtected = _publicAccessService.IsProtected(entity.Path);
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
    //            .CalculateContentStartNodeIds(EntityService, _appCaches)
    //        ?? Array.Empty<int>();
    //
    // protected override string[] GetUserStartNodePaths()
    //     => _backofficeSecurityAccessor
    //            .BackOfficeSecurity?
    //            .CurrentUser?
    //            .GetContentStartNodePaths(EntityService, _appCaches)
    //        ?? Array.Empty<string>();
}
