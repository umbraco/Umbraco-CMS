using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;
using Umbraco.Cms.Api.Delivery.Routing;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Controllers.Security;

[ApiVersion("1.0")]
[ApiController]
[VersionedDeliveryApiRoute(Common.Security.Paths.MemberApi.EndpointTemplate)]
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
public class CurrentMemberController : DeliveryApiControllerBase
{
    private readonly ICurrentMemberClaimsProvider _currentMemberClaimsProvider;

    public CurrentMemberController(ICurrentMemberClaimsProvider currentMemberClaimsProvider)
        => _currentMemberClaimsProvider = currentMemberClaimsProvider;

    [HttpGet("userinfo")]
    public async Task<IActionResult> Userinfo()
    {
        Dictionary<string, object> claims = await _currentMemberClaimsProvider.GetClaimsAsync();
        return Ok(claims);
    }
}
