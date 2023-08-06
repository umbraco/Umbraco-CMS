using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.RecycleBin;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.ViewModels.RecycleBin;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document.RecycleBin;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.RecycleBin}/{Constants.UdiEntityType.Document}")]
[RequireDocumentTreeRootAccess]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDocuments)]
public class DocumentRecycleBinControllerBase : RecycleBinControllerBase<RecycleBinItemResponseModel>
{
    public DocumentRecycleBinControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Document;

    protected override int RecycleBinRootId => Constants.System.RecycleBinContent;

    protected override RecycleBinItemResponseModel MapRecycleBinViewModel(Guid? parentId, IEntitySlim entity)
    {
        RecycleBinItemResponseModel responseModel = base.MapRecycleBinViewModel(parentId, entity);

        if (entity is IDocumentEntitySlim documentEntitySlim)
        {
            responseModel.Icon = documentEntitySlim.ContentTypeIcon ?? responseModel.Icon;
        }

        return responseModel;
    }
}
