// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     The resource used for the <see cref="ContentPermissionsResourceRequirement" />
/// </summary>
public class ContentPermissionsResource
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionsResource" /> class.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    public ContentPermissionsResource(IContent? content, string permissionToCheck)
    {
        PermissionsToCheck = new HashSet<string> { permissionToCheck };
        Content = content;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionsResource" /> class.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    public ContentPermissionsResource(IContent content, IReadOnlySet<string> permissionsToCheck)
    {
        Content = content;
        PermissionsToCheck = permissionsToCheck;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionsResource" /> class.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="nodeId">The node Id.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    public ContentPermissionsResource(IContent? content, int nodeId, IReadOnlySet<string> permissionsToCheck)
    {
        Content = content;
        NodeId = nodeId;
        PermissionsToCheck = permissionsToCheck;
    }

    /// <summary>
    ///     Gets the node Id.
    /// </summary>
    public int? NodeId { get; }

    /// <summary>
    ///     Gets the collection of permissions to authorize.
    /// </summary>
    public IReadOnlySet<string> PermissionsToCheck { get; }

    /// <summary>
    ///     Gets the content.
    /// </summary>
    public IContent? Content { get; }
}
