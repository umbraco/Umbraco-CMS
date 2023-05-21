using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     A bare minimum structure that represents a user, usually attached to other objects
/// </summary>
[DataContract(Name = "user", Namespace = "")]
public class UserProfile : IComparable
{
    [DataMember(Name = "id", IsRequired = true)]
    [Required]
    public int UserId { get; set; }

    [DataMember(Name = "name", IsRequired = true)]
    [Required]
    public string? Name { get; set; }

    int IComparable.CompareTo(object? obj) => string.Compare(Name, ((UserProfile?)obj)?.Name, StringComparison.Ordinal);
}
