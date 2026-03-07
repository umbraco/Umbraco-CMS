using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.ObjectTypes;

    /// <summary>
    /// Serves as the base controller for handling operations related to object types in the Umbraco CMS Management API.
    /// </summary>
[VersionedApiBackOfficeRoute("object-types")]
[ApiExplorerSettings(GroupName = "Object Types")]
public class ObjectTypesControllerBase : ManagementApiControllerBase
{
}
