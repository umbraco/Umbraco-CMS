namespace Umbraco.Cms.Core.PropertyEditors;

public class UserPickerConfiguration : ConfigurationEditor
{
    public override IDictionary<string, object> DefaultConfiguration => new Dictionary<string, object>
    {
        { "entityType", "User" }, { "multiPicker", "0" },
    };
}
