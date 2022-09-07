using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Document.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Document}/tree")]
public abstract class DocumentTreeControllerBase : ContentTreeControllerBase<DocumentTreeItemViewModel>
{
    private readonly IPublicAccessService _publicAccessService;

    protected DocumentTreeControllerBase(
        IEntityService entityService,
        IPublicAccessService publicAccessService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        : base(entityService, appCaches, backofficeSecurityAccessor) =>
        _publicAccessService = publicAccessService;

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

    // TODO: delete (faking start node setup for unlimited editor)
    // protected override int[] GetUserStartNodeIds() => new[] { -1 };
    //
    // protected override string[] GetUserStartNodePaths() => Array.Empty<string>();

    // TODO: delete (faking start node setup for limited editor)
    protected override int[] GetUserStartNodeIds() => new[] { 1078, 1083 };

    protected override string[] GetUserStartNodePaths() => new[] { "-1,1056,1068,1078", "-1,1082,1083" };

    // TODO: use these implementations instead of the dummy ones above
    // protected override int[] GetUserStartNodeIds()
    //     => BackofficeSecurityAccessor
    //            .BackOfficeSecurity?
    //            .CurrentUser?
    //            .CalculateContentStartNodeIds(EntityService, AppCaches)
    //        ?? Array.Empty<int>();
    //
    // protected override string[] GetUserStartNodePaths()
    //     => BackofficeSecurityAccessor
    //            .BackOfficeSecurity?
    //            .CurrentUser?
    //            .GetContentStartNodePaths(EntityService, AppCaches)
    //        ?? Array.Empty<string>();
}
