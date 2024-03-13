using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Folder;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.DataType}/folder")]
[ApiExplorerSettings(GroupName = "Data Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public abstract class DataTypeFolderControllerBase : FolderManagementControllerBase<IDataType>
{
    protected DataTypeFolderControllerBase(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDataTypeContainerService dataTypeContainerService)
        : base(backOfficeSecurityAccessor, dataTypeContainerService)
    {
    }
}
