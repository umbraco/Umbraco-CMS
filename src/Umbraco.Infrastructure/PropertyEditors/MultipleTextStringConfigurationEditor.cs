// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for a multiple textstring value editor.
/// </summary>
internal class MultipleTextStringConfigurationEditor : ConfigurationEditor<MultipleTextStringConfiguration>
{
    public MultipleTextStringConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
        Fields.Add(new ConfigurationField(new IntegerValidator())
        {
            Key = "min",
            PropertyName = nameof(MultipleTextStringConfiguration.Min),
        });

        Fields.Add(new ConfigurationField(new IntegerValidator())
        {
            Key = "max",
            PropertyName = nameof(MultipleTextStringConfiguration.Max),
        });
    }
}
