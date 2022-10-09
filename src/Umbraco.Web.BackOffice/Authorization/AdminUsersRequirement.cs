// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     Authorization requirement for the <see cref="AdminUsersHandler" />
/// </summary>
public class AdminUsersRequirement : IAuthorizationRequirement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AdminUsersRequirement" /> class.
    /// </summary>
    /// <param name="queryStringName">Query string name from which to authorize values.</param>
    public AdminUsersRequirement(string queryStringName = "id") => QueryStringName = queryStringName;

    /// <summary>
    ///     Gets the query string name from which to authorize values.
    /// </summary>
    public string QueryStringName { get; }
}
