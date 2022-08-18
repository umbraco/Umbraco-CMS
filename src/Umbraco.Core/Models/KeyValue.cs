using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Implements <see cref="IKeyValue" />.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class KeyValue : EntityBase, IKeyValue
{
    private string _identifier = null!;
    private string? _value;

    /// <inheritdoc />
    public string Identifier
    {
        get => _identifier;
        set => SetPropertyValueAndDetectChanges(value, ref _identifier!, nameof(Identifier));
    }

    /// <inheritdoc />
    public string? Value
    {
        get => _value;
        set => SetPropertyValueAndDetectChanges(value, ref _value, nameof(Value));
    }

    bool IEntity.HasIdentity => !string.IsNullOrEmpty(Identifier);
}
