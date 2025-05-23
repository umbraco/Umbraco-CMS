using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     A custom pre-value editor class to deal with the legacy way that the pre-value data is stored.
/// </summary>
public class IntegerConfigurationEditor : ConfigurationEditor
{
    public IntegerConfigurationEditor()
    {
        Fields.Add(new ConfigurationField(new IntegerValidator())
        {
            Key = "min",
        });

        Fields.Add(new ConfigurationField(new IntegerValidator())
        {
            Key = "step",
        });

        Fields.Add(new ConfigurationField(new IntegerValidator())
        {
            Key = "max",
        });
    }
}
