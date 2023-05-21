// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     An authorization requirement for <see cref="MediaPermissionsQueryStringHandler" />
/// </summary>
public class MediaPermissionsQueryStringRequirement : IAuthorizationRequirement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaPermissionsQueryStringRequirement" /> class.
    /// </summary>
    /// <param name="paramName">Querystring paramter name.</param>
    public MediaPermissionsQueryStringRequirement(string paramName) => QueryStringName = paramName;

    /// <summary>
    ///     Gets the querystring paramter name.
    /// </summary>
    public string QueryStringName { get; }
}
