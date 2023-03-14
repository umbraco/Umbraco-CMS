namespace Umbraco.Cms.Core.PropertyEditors;

public class MemberPickerConfiguration : ConfigurationEditor
{
    public override IDictionary<string, object> DefaultConfiguration =>
        new Dictionary<string, object> { { "idType", "udi" } };
}
