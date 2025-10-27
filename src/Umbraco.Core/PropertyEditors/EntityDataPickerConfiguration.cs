namespace Umbraco.Cms.Core.PropertyEditors;

public sealed class EntityDataPickerConfiguration
{
    [ConfigurationField("validationLimit")]
    public NumberRange ValidationLimit { get; set; } = new();

    [ConfigurationField("umbEditorDataSource")]
    public string DataSource { get; set; } = string.Empty;

    public class NumberRange
    {
        public int? Min { get; set; }

        public int? Max { get; set; }
    }
}
