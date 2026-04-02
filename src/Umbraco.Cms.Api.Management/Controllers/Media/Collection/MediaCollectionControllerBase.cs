using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Controllers.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Collection;

/// <summary>
/// Serves as the base controller for media collection management in the Umbraco CMS API.
/// Provides shared functionality for media collection operations.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Collection}/{Constants.UdiEntityType.Media}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Media))]
[Authorize(Policy = AuthorizationPolicies.SectionAccessMedia)]
public abstract class MediaCollectionControllerBase : ContentCollectionControllerBase<IMedia, MediaCollectionResponseModel, MediaValueResponseModel, MediaVariantResponseModel>
{
    protected MediaCollectionControllerBase(IUmbracoMapper mapper, FlagProviderCollection flagProviders)
        : base(mapper, flagProviders)
    {
    }

    protected IActionResult CollectionOperationStatusResult(ContentCollectionOperationStatus status)
        => ContentCollectionOperationStatusResult(status, "media");
}
