namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the Entity Data Picker property editor.
/// </summary>
public sealed class EntityDataPickerConfiguration
{
    /// <summary>
    ///     Gets or sets the validation limit for the number of selected entities.
    /// </summary>
    [ConfigurationField("validationLimit", Type = typeof(RangeConfigurationField))]
    public PropertyEditors.NumberRange ValidationLimit { get; set; } = new();

    /// <summary>
    ///     Gets or sets the data source identifier for the entity picker.
    /// </summary>
    [ConfigurationField("umbEditorDataSource")]
    public string DataSource { get; set; } = string.Empty;

    /// <summary>
    ///     Represents a number range with optional minimum and maximum values.
    /// </summary>
    [Obsolete("No longer used by Umbraco; use Umbraco.Cms.Core.PropertyEditors.NumberRange instead. Scheduled for removal in Umbraco 21.")]
    public class NumberRange : PropertyEditors.NumberRange
    {
    }
}
