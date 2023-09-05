namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a non configurable Configuration for a string value type
/// </summary>
public class NoopStringValueTypeConfiguration : IConfigureValueType
{
    public string ValueType { get; set; } = ValueTypes.String;
}
