namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the ValueList editor configuration.
/// </summary>
public class ValueListConfiguration
{
    /// <summary>
    ///     Gets or sets the list of selectable items.
    /// </summary>
    [ConfigurationField("items")]
    public List<string> Items { get; set; } = new();
}
