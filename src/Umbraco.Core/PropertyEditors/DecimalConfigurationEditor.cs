using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     A custom pre-value editor class to deal with the legacy way that the pre-value data is stored.
/// </summary>
public class DecimalConfigurationEditor : ConfigurationEditor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DecimalConfigurationEditor"/> class.
    /// </summary>
    public DecimalConfigurationEditor()
    {
        Fields.Add(new ConfigurationField(new DecimalValidator())
        {
            Key = "min"
        });

        Fields.Add(new ConfigurationField(new DecimalValidator())
        {
            Key = "step",
        });

        Fields.Add(new ConfigurationField(new DecimalValidator())
        {
            Key = "max",
        });
    }
}
