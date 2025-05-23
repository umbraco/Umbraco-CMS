using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.DenyLocalLogin;

/// <summary>
///     Marker requirement for the <see cref="DenyLocalLoginHandler" />.
/// </summary>
public class DenyLocalLoginRequirement : IAuthorizationRequirement
{
}
