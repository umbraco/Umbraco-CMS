using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Folder;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Element}/folder")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Element))]
// TODO ELEMENTS: backoffice authorization policies
public abstract class ElementFolderControllerBase : FolderManagementControllerBase<IElement>
{
    protected ElementFolderControllerBase(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IElementContainerService elementContainerService)
        : base(backOfficeSecurityAccessor, elementContainerService)
    {
    }
}
