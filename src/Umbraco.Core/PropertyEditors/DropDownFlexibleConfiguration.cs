namespace Umbraco.Cms.Core.PropertyEditors;

public class DropDownFlexibleConfiguration : ValueListConfiguration
{
    [ConfigurationField("multiple")]
    public bool Multiple { get; set; }
}
