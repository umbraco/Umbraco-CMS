using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Template}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Template))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessTemplates)]
public class TemplateTreeControllerBase : NamedEntityTreeControllerBase<NamedEntityTreeItemResponseModel>
{
    public TemplateTreeControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Template;
}
