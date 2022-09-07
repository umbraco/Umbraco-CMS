using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.MemberType.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.MemberType}/tree")]
public class MemberTypeTreeControllerBase : TreeControllerBase<TreeItemViewModel>
{
    private readonly IContentTypeService _contentTypeService;

    public MemberTypeTreeControllerBase(IEntityService entityService, IContentTypeService contentTypeService)
        : base(entityService) =>
        _contentTypeService = contentTypeService;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.MemberType;

    protected override TreeItemViewModel MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        TreeItemViewModel viewModel = base.MapTreeItemViewModel(parentKey, entity);
        viewModel.Icon = "icon-user";
        return viewModel;
    }
}
