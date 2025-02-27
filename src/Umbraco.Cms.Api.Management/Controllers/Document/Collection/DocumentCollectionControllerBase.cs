using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Collection;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Collection}/{Constants.UdiEntityType.Document}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocuments)]
public abstract class DocumentCollectionControllerBase : ContentCollectionControllerBase<IContent, DocumentCollectionResponseModel, DocumentValueResponseModel, DocumentVariantResponseModel>
{
    protected DocumentCollectionControllerBase(IUmbracoMapper mapper)
        : base(mapper)
    {
    }

    protected IActionResult CollectionOperationStatusResult(ContentCollectionOperationStatus status)
        => ContentCollectionOperationStatusResult(status, "document");
}
