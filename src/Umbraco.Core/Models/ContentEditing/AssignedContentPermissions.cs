using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     The permissions assigned to a content node
/// </summary>
/// <remarks>
///     The underlying <see cref="EntityBasic" /> data such as Name, etc... is that of the Content item
/// </remarks>
[DataContract(Name = "contentPermissions", Namespace = "")]
public class AssignedContentPermissions : EntityBasic
{
    /// <summary>
    ///     The assigned permissions to the content item organized by permission group name
    /// </summary>
    [DataMember(Name = "permissions")]
    public IDictionary<string, IEnumerable<Permission>>? AssignedPermissions { get; set; }
}
