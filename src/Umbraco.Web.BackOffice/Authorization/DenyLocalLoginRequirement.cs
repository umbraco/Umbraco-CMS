// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     Marker requirement for the <see cref="DenyLocalLoginHandler" />.
/// </summary>
public class DenyLocalLoginRequirement : IAuthorizationRequirement
{
}
