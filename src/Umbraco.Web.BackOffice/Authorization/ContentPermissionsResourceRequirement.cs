// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// An authorization requirement for <see cref="ContentPermissionsResourceHandler"/>
    /// </summary>
    public class ContentPermissionsResourceRequirement : IAuthorizationRequirement
    {
    }
}
