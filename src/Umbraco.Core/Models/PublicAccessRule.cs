using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a rule that defines access permissions for a public access entry.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class PublicAccessRule : EntityBase
{
    private string? _ruleType;
    private string? _ruleValue;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublicAccessRule" /> class with the specified identifiers.
    /// </summary>
    /// <param name="id">The unique identifier of the rule.</param>
    /// <param name="accessEntryId">The unique identifier of the associated access entry.</param>
    public PublicAccessRule(Guid id, Guid accessEntryId)
    {
        AccessEntryId = accessEntryId;
        Key = id;
        Id = Key.GetHashCode();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublicAccessRule" /> class.
    /// </summary>
    public PublicAccessRule()
    {
    }

    /// <summary>
    ///     Gets or sets the unique identifier of the associated public access entry.
    /// </summary>
    public Guid AccessEntryId { get; set; }

    /// <summary>
    ///     Gets or sets the value of the rule (e.g., member username or group name).
    /// </summary>
    public string? RuleValue
    {
        get => _ruleValue;
        set => SetPropertyValueAndDetectChanges(value, ref _ruleValue, nameof(RuleValue));
    }

    /// <summary>
    ///     Gets or sets the type of the rule (e.g., "Username" or "MemberType").
    /// </summary>
    public string? RuleType
    {
        get => _ruleType;
        set => SetPropertyValueAndDetectChanges(value, ref _ruleType, nameof(RuleType));
    }
}
