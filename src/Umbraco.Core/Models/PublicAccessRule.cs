using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

[Serializable]
[DataContract(IsReference = true)]
public class PublicAccessRule : EntityBase
{
    private string? _ruleType;
    private string? _ruleValue;

    public PublicAccessRule(Guid id, Guid accessEntryId)
    {
        AccessEntryId = accessEntryId;
        Key = id;
        Id = Key.GetHashCode();
    }

    public PublicAccessRule()
    {
    }

    public Guid AccessEntryId { get; set; }

    public string? RuleValue
    {
        get => _ruleValue;
        set => SetPropertyValueAndDetectChanges(value, ref _ruleValue, nameof(RuleValue));
    }

    public string? RuleType
    {
        get => _ruleType;
        set => SetPropertyValueAndDetectChanges(value, ref _ruleType, nameof(RuleType));
    }
}
