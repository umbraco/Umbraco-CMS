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
    public Guid? DocumentStartNodeId { get; init; }

    /// <summary>
    /// The Id of the media that should act as root node for the user group
    /// <remarks>
    /// This can be overwritten by a different user group if a user is a member of multiple groups
    /// </remarks>
    /// </summary>
    public Guid? MediaStartNodeId { get; init; }

    /// <summary>
    /// Ad-hoc list of permissions provided, and maintained by the front-end. The server has no concept of what these mean.
    /// </summary>
    public required ISet<string> Permissions { get; init; }
}
