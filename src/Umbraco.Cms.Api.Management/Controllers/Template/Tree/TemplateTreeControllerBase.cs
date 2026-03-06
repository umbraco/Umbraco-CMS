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

/// <summary>
/// Serves as the base controller for handling operations related to template trees in the Umbraco CMS Management API.
/// Provides common functionality for derived controllers managing template structures.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Template}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Template))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessTemplates)]
public class TemplateTreeControllerBase : NamedEntityTreeControllerBase<NamedEntityTreeItemResponseModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateTreeControllerBase"/> class, which provides base functionality for template tree controllers.
    /// </summary>
    /// <param name="entityService">An <see cref="IEntityService"/> instance used for managing entities within the controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public TemplateTreeControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateTreeControllerBase"/> class with the specified entity service and flag providers.
    /// </summary>
    /// <param name="entityService">The <see cref="IEntityService"/> used for entity operations within the template tree controller.</param>
    /// <param name="flagProviders">A collection of <see cref="FlagProviderCollection"/> used to provide additional flags or metadata for entities.</param>
    public TemplateTreeControllerBase(
        IEntityService entityService, 
        FlagProviderCollection flagProviders)
        : base(entityService, flagProviders)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Template;
}
