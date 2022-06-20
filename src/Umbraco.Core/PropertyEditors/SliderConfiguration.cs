namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the slider value editor.
/// </summary>
public class SliderConfiguration
{
    [ConfigurationField("enableRange", "Enable range", "boolean")]
    public bool EnableRange { get; set; }

    [ConfigurationField("initVal1", "Initial value", "number")]
    public decimal InitialValue { get; set; }

    [ConfigurationField("initVal2", "Initial value 2", "number", Description = "Used when range is enabled")]
    public decimal InitialValue2 { get; set; }

    [ConfigurationField("minVal", "Minimum value", "number")]
    public decimal MinimumValue { get; set; }

    [ConfigurationField("maxVal", "Maximum value", "number")]
    public decimal MaximumValue { get; set; }

    [ConfigurationField("step", "Step increments", "number")]
    public decimal StepIncrements { get; set; }
}
