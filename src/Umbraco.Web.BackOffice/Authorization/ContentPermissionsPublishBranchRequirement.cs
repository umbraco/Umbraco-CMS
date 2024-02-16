// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     Authorization requirement for <see cref="ContentPermissionsPublishBranchHandler" />
/// </summary>
public class ContentPermissionsPublishBranchRequirement : IAuthorizationRequirement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionsPublishBranchRequirement" /> class.
    /// </summary>
    /// <param name="permission">Permission to check.</param>
    public ContentPermissionsPublishBranchRequirement(string permission) => Permission = permission;

    /// <summary>
    ///     Gets a value for the permission to check.
    /// </summary>
    public string Permission { get; }
}
