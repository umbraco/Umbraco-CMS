namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the ValueList editor configuration.
/// </summary>
public class ValueListConfiguration
{
    /// <summary>
    ///     Gets or sets the list of selectable options.
    /// </summary>
    [ConfigurationField("items")]
    public List<ValueListOption> Items
    {
        get;
        set => field = value ?? [];
    } = [];
}
