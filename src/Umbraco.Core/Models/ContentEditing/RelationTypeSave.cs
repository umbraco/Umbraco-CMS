using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "relationType", Namespace = "")]
public class RelationTypeSave : EntityBasic
{
    /// <summary>
    ///     Gets or sets a boolean indicating whether the RelationType is Bidirectional (true) or Parent to Child (false)
    /// </summary>
    [DataMember(Name = "isBidirectional", IsRequired = true)]
    public bool IsBidirectional { get; set; }

    /// <summary>
    ///     Gets or sets the parent object type ID.
    /// </summary>
    [DataMember(Name = "parentObjectType", IsRequired = false)]
    public Guid? ParentObjectType { get; set; }

    /// <summary>
    ///     Gets or sets the child object type ID.
    /// </summary>
    [DataMember(Name = "childObjectType", IsRequired = false)]
    public Guid? ChildObjectType { get; set; }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the RelationType should be returned in "Used by"-queries.
    /// </summary>
    [DataMember(Name = "isDependency", IsRequired = true)]
    public bool IsDependency { get; set; }
}
