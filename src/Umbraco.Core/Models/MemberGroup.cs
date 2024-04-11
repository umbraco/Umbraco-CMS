using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a member type
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class MemberGroup : EntityBase, IMemberGroup
{
    private int _creatorId;
    private string? _name;

    [DataMember]
    public string? Name
    {
        get => _name;
        set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
    }

    [DataMember]
    public int CreatorId
    {
        get => _creatorId;
        set => SetPropertyValueAndDetectChanges(value, ref _creatorId, nameof(CreatorId));
    }
}
