namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the ValueList editor configuration.
/// </summary>
public class ValueListConfiguration
{
    [ConfigurationField("items")]
    public List<ValueListItem> Items { get; set; } = new();

    public class ValueListItem
    {
        public string? Value { get; set; }
    }
}
