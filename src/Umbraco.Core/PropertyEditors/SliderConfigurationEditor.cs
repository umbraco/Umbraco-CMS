// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the slider value editor.
/// </summary>
public class SliderConfigurationEditor : ConfigurationEditor<SliderConfiguration>
{
    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public SliderConfigurationEditor(IIOHelper ioHelper)
        : this(ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public SliderConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(
        ioHelper, editorConfigurationParser)
    {
    }

    public override Dictionary<string, object> ToConfigurationEditor(SliderConfiguration? configuration)
    {
        // negative step increments can be configured in the back-office. they will cause the slider to
        // crash the entire back-office. as we can't configure min and max values for the number prevalue
        // editor, we have to this instead to limit the damage.
        // logically, the step increments should be inverted instead of hardcoding them to 1, but the
        // latter might point people in the direction of their misconfiguration.
        if (configuration?.StepIncrements <= 0)
        {
            configuration.StepIncrements = 1;
        }

        return base.ToConfigurationEditor(configuration);
    }
}
