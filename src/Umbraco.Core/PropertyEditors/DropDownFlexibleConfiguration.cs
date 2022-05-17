namespace Umbraco.Cms.Core.PropertyEditors;

public class DropDownFlexibleConfiguration : ValueListConfiguration
{
    [ConfigurationField(
        "multiple",
        "Enable multiple choice",
        "boolean",
        Description = "When checked, the dropdown will be a select multiple / combo box style dropdown.")]
    public bool Multiple { get; set; }
}
