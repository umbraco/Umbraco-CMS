namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the Entity Data Picker property editor.
/// </summary>
public sealed class EntityDataPickerConfiguration
{
    /// <summary>
    ///     Gets or sets the validation limit for the number of selected entities.
    /// </summary>
    [ConfigurationField("validationLimit")]
    public NumberRange ValidationLimit { get; set; } = new();

    /// <summary>
    ///     Gets or sets the data source identifier for the entity picker.
    /// </summary>
    [ConfigurationField("umbEditorDataSource")]
    public string DataSource { get; set; } = string.Empty;

    /// <summary>
    ///     Represents a number range with optional minimum and maximum values.
    /// </summary>
    public class NumberRange
    {
        /// <summary>
        ///     Gets or sets the minimum number of items allowed.
        /// </summary>
        public int? Min { get; set; }

        /// <summary>
        ///     Gets or sets the maximum number of items allowed.
        /// </summary>
        public int? Max { get; set; }
    }
}
