// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     An authorization requirement for <see cref="ContentPermissionsQueryStringHandler" />
/// </summary>
public class ContentPermissionsQueryStringRequirement : IAuthorizationRequirement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionsQueryStringRequirement" /> class for a specific node
    ///     id.
    /// </summary>
    /// <param name="nodeId">The node Id.</param>
    /// <param name="permissionToCheck">The permission to authorize the current user against.</param>
    public ContentPermissionsQueryStringRequirement(int nodeId, char permissionToCheck)
    {
        NodeId = nodeId;
        PermissionToCheck = permissionToCheck;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionsQueryStringRequirement" /> class for a
    ///     node id based on a query string parameter.
    /// </summary>
    /// <param name="paramName">The querystring parameter name.</param>
    /// <param name="permissionToCheck">The permission to authorize the current user against.</param>
    public ContentPermissionsQueryStringRequirement(char permissionToCheck, string paramName = "id")
    {
        QueryStringName = paramName;
        PermissionToCheck = permissionToCheck;
    }

    /// <summary>
    ///     Gets the specific node Id.
    /// </summary>
    public int? NodeId { get; }

    /// <summary>
    ///     Gets the querystring parameter name.
    /// </summary>
    public string? QueryStringName { get; }

    /// <summary>
    ///     Gets the permission to authorize the current user against.
    /// </summary>
    public char PermissionToCheck { get; }
}
