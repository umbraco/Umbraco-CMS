namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the ValueList editor configuration.
/// </summary>
public class ValueListConfiguration
{
    [ConfigurationField("items")]
    public List<string> Items { get; set; } = new();
}
