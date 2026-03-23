using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup;

/// <summary>
/// <para>
/// Base class for front-end representation of a User Group.
/// </para>
///  <para>
/// Contains all the properties shared between Save, Update, Representation, etc...
/// </para>
/// </summary>
public class UserGroupBase
{
    /// <summary>
    /// Gets or sets the name of the user group.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the alias of the user group.
    /// </summary>
    public required string Alias { get; init; }

    /// <summary>
    /// Gets or sets the description of the user group.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the icon for the user group.
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// Gets or sets the sections that the user group has access to.
    /// </summary>
    public required IEnumerable<string> Sections { get; init; }

    /// <summary>
    /// Gets or sets the languages that the user group has access to.
    /// </summary>
    public required IEnumerable<string> Languages { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the user group gives access to all languages, regardless of <see cref="UserGroupBase.Languages"/>.
    /// </summary>
    public required bool HasAccessToAllLanguages { get; init; }

    /// <summary>
    /// Gets or sets the key of the document that should act as root node for the user group.
    /// <remarks>
    /// This can be overwritten by a different user group if a user is a member of multiple groups
    /// </remarks>
    /// </summary>
    public ReferenceByIdModel? DocumentStartNode { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the group should have access to the document root.
    /// <remarks>
    /// This will be ignored if an explicit start node has been specified in <see cref="DocumentStartNode"/>.
    /// </remarks>
    /// </summary>
    public bool DocumentRootAccess { get; init; }

    /// <summary>
    /// Gets or sets the ID of the media that should act as root node for the user group.
    /// <remarks>
    /// This can be overwritten by a different user group if a user is a member of multiple groups
    /// </remarks>
    /// </summary>
    public ReferenceByIdModel? MediaStartNode { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the group should have access to the media root.
    /// <remarks>
    /// This will be ignored if an explicit start node has been specified in <see cref="MediaStartNode"/>.
    /// </remarks>
    /// </summary>
    public bool MediaRootAccess { get; init; }

    /// <summary>
    /// Gets or sets the list of permissions provided and maintained by the front-end. The server has no concept of all of them, but some can be used on the server.
    /// </summary>
    public required ISet<string> FallbackPermissions { get; init; }
    /// <summary>
    /// Gets or sets the set of permissions associated with the user group.
    /// </summary>
    public required ISet<IPermissionPresentationModel> Permissions { get; init; }
}
