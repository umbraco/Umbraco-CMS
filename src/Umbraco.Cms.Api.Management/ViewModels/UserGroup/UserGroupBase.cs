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
    /// The name of the user groups
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The alias of the user groups
    /// </summary>
    public required string Alias { get; init; }

    /// <summary>
    /// The Icon for the user group
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// The sections that the user group has access to
    /// </summary>
    public required IEnumerable<string> Sections { get; init; }

    /// <summary>
    /// The languages that the user group has access to
    /// </summary>
    public required IEnumerable<string> Languages { get; init; }

    /// <summary>
    /// Flag indicating if the user group gives access to all languages, regardless of <see cref="UserGroupBase.Languages"/>.
    /// </summary>
    public required bool HasAccessToAllLanguages { get; init; }

    /// <summary>
    /// The key of the document that should act as root node for the user group
    /// <remarks>
    /// This can be overwritten by a different user group if a user is a member of multiple groups
    /// </remarks>
    /// </summary>
    public ReferenceByIdModel? DocumentStartNode { get; init; }

    /// <summary>
    /// If the group should have access to the document root.
    /// <remarks>
    /// This will be ignored if an explicit start node has been specified in <see cref="DocumentStartNode"/>.
    /// </remarks>
    /// </summary>
    public bool DocumentRootAccess { get; init; }

    /// <summary>
    /// The Id of the media that should act as root node for the user group
    /// <remarks>
    /// This can be overwritten by a different user group if a user is a member of multiple groups
    /// </remarks>
    /// </summary>
    public ReferenceByIdModel? MediaStartNode { get; init; }

    /// <summary>
    /// If the group should have access to the media root.
    /// <remarks>
    /// This will be ignored if an explicit start node has been specified in <see cref="MediaStartNode"/>.
    /// </remarks>
    /// </summary>
    public bool MediaRootAccess { get; init; }

    /// <summary>
    /// List of permissions provided, and maintained by the front-end. The server has no concept all of them, but some can be used on the server.
    /// </summary>
    public required ISet<string> FallbackPermissions { get; init; }
    public required ISet<IPermissionPresentationModel> Permissions { get; init; }
}
