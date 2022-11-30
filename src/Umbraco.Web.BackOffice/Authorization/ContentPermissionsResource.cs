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
    public ContentPermissionsResource(IContent? content, char permissionToCheck)
    {
        PermissionsToCheck = new List<char> { permissionToCheck };
        Content = content;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionsResource" /> class.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    public ContentPermissionsResource(IContent content, IReadOnlyList<char> permissionsToCheck)
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
    public ContentPermissionsResource(IContent? content, int nodeId, IReadOnlyList<char> permissionsToCheck)
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
    public IReadOnlyList<char> PermissionsToCheck { get; }

    /// <summary>
    ///     Gets the content.
    /// </summary>
    public IContent? Content { get; }
}
