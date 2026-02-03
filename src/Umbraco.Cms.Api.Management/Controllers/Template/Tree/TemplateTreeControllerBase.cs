using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Flags;
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
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public TemplateTreeControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    public TemplateTreeControllerBase(IEntityService entityService, FlagProviderCollection flagProviders)
        : base(entityService, flagProviders)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Template;
    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.Unknown;
}
