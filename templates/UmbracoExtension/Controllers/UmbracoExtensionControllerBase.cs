using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;

namespace PROJECT_SAFENAME.Controllers
{
    [ApiController]
    [BackOfficeRoute("PROJECT_SAFENAME/api/v{version:apiVersion}")]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
    [MapToApi(Constants.ApiName)]
    public class PROJECT_SAFENAMEControllerBase : ControllerBase
    {
    }
}
