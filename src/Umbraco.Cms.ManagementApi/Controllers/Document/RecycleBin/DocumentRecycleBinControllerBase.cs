using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Controllers.RecycleBin;
using Umbraco.Cms.ManagementApi.Filters;
using Umbraco.Cms.ManagementApi.ViewModels.RecycleBin;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Document.RecycleBin;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Document}/recycle-bin")]
[RequireDocumentTreeRootAccess]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[OpenApiTag(nameof(Constants.UdiEntityType.Document))]
public class DocumentRecycleBinControllerBase : RecycleBinControllerBase<RecycleBinItemViewModel>
{
    public DocumentRecycleBinControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Document;

    protected override int RecycleBinRootId => Constants.System.RecycleBinContent;

    protected override RecycleBinItemViewModel MapRecycleBinViewModel(Guid? parentKey, IEntitySlim entity)
    {
        RecycleBinItemViewModel viewModel = base.MapRecycleBinViewModel(parentKey, entity);

        if (entity is IDocumentEntitySlim documentEntitySlim)
        {
            viewModel.Icon = documentEntitySlim.ContentTypeIcon ?? viewModel.Icon;
        }

        return viewModel;
    }
}
