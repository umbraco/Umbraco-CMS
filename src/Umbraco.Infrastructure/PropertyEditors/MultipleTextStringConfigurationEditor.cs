// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
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
            PropertyName = nameof(MultipleTextStringConfiguration.Min),
        });

        Fields.Add(new ConfigurationField(new IntegerValidator())
        {
            Description = "Enter the maximum amount of text boxes to be displayed, enter 0 for unlimited",
            Key = "max",
            View = "requiredfield",
            Name = "Maximum",
            PropertyName = nameof(MultipleTextStringConfiguration.Max),
        });
    }
}
