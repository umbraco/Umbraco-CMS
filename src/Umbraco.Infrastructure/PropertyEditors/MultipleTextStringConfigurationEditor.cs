// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for a multiple textstring value editor.
/// </summary>
internal class MultipleTextStringConfigurationEditor : ConfigurationEditor<MultipleTextStringConfiguration>
{
    public MultipleTextStringConfigurationEditor(
        IIOHelper ioHelper,
        IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
        Fields.Add(new ConfigurationField(new IntegerValidator())
        {
            Description = "Enter the minimum amount of text boxes to be displayed",
            Key = "min",
            View = "requiredfield",
            Name = "Minimum",
            PropertyName = nameof(MultipleTextStringConfiguration.Minimum),
        });

        Fields.Add(new ConfigurationField(new IntegerValidator())
        {
            Description = "Enter the maximum amount of text boxes to be displayed, enter 0 for unlimited",
            Key = "max",
            View = "requiredfield",
            Name = "Maximum",
            PropertyName = nameof(MultipleTextStringConfiguration.Maximum),
        });
    }

    /// <inheritdoc />
    public override MultipleTextStringConfiguration FromConfigurationEditor(
        IDictionary<string, object?>? editorValues,
        MultipleTextStringConfiguration? configuration)
    {
        // TODO: this isn't pretty
        // the values from the editor will be min/max fields and we need to format to json in one field
        // is the editor sending strings or ints or?!
        Attempt<int> min = (editorValues?.ContainsKey("min") ?? false ? editorValues["min"]?.ToString() : "0")
            .TryConvertTo<int>();
        Attempt<int> max = (editorValues?.ContainsKey("max") ?? false ? editorValues["max"]?.ToString() : "0")
            .TryConvertTo<int>();

        return new MultipleTextStringConfiguration
        {
            Minimum = min.Success ? min.Result : 0,
            Maximum = max.Success ? max.Result : 0,
        };
    }

    /// <inheritdoc />
    public override Dictionary<string, object> ToConfigurationEditor(MultipleTextStringConfiguration? configuration) =>
        new Dictionary<string, object> { { "min", configuration?.Minimum ?? 0 }, { "max", configuration?.Maximum ?? 0 } };
}
