using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Filter;

/// <summary>
/// Serves as the base controller for implementing member filtering operations in the API.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Filter}/{Constants.UdiEntityType.Member}")]
public abstract class MemberFilterControllerBase : MemberControllerBase
{
    protected IActionResult MemberTypeNotFound()
        => OperationStatusResult(ContentEditingOperationStatus.NotFound, problemDetailsBuilder
            => NotFound(problemDetailsBuilder
                .WithTitle("The requested member type could not be found")
                .Build()));
}
