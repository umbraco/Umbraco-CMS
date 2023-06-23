// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the datetime value editor.
/// </summary>
public class DateTimeConfigurationEditor : ConfigurationEditor<DateTimeConfiguration>
{
    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public DateTimeConfigurationEditor(IIOHelper ioHelper)
        : this(
        ioHelper,
        StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public DateTimeConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(
        ioHelper, editorConfigurationParser)
    {
    }

    public override IDictionary<string, object> ToValueEditor(IDictionary<string, object> configuration)
    {
        IDictionary<string, object> config = base.ToValueEditor(configuration);

        // FIXME: all this belongs clientside, this needs to go
        var pickTime = true;
        if (config.TryGetValue("format", out object? formatValue) && formatValue is string format)
        {
            pickTime = format.ContainsAny(new[] { "H", "m", "s" });
        }

        config["pickTime"] = pickTime;

        return config;
    }
}
