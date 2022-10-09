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
            Description = "Enter the minimum amount of number to be entered",
            Key = "min",
            View = "number",
            Name = "Minimum",
        });

        Fields.Add(new ConfigurationField(new IntegerValidator())
        {
            Description = "Enter the intervals amount between each step of number to be entered",
            Key = "step",
            View = "number",
            Name = "Step Size",
        });

        Fields.Add(new ConfigurationField(new IntegerValidator())
        {
            Description = "Enter the maximum amount of number to be entered",
            Key = "max",
            View = "number",
            Name = "Maximum",
        });
    }
}
