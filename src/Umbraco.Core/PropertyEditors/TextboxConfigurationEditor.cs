// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the textbox value editor.
/// </summary>
public class TextboxConfigurationEditor : ConfigurationEditor<TextboxConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextboxConfigurationEditor"/> class.
    /// </summary>
    public TextboxConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
        const int MinChars = 1;
        const int MaxChars = 512;
        Fields.Add(new ConfigurationField(new IntegerValidator(MinChars, MaxChars))
        {
            Key = "maxChars",
        });
    }
}
