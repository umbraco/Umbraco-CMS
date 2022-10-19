// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     Authorization requirement for the <see cref="UserGroupHandler" />
/// </summary>
public class UserGroupRequirement : IAuthorizationRequirement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupRequirement" /> class.
    /// </summary>
    /// <param name="queryStringName">Query string name from which to authorize values.</param>
    public UserGroupRequirement(string queryStringName = "id") => QueryStringName = queryStringName;

    /// <summary>
    ///     Gets the query string name from which to authorize values.
    /// </summary>
    public string QueryStringName { get; }
}
