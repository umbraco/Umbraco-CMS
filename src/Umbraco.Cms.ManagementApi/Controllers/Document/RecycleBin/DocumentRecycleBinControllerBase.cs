using Microsoft.AspNetCore.Mvc;
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
[RequireDocumentTreeRootAccess] // NOTE: this is pending evaluation for new backoffice
public class DocumentRecycleBinControllerBase : RecycleBinControllerBase<RecycleBinItemViewModel>
{
    public DocumentRecycleBinControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Document;

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
