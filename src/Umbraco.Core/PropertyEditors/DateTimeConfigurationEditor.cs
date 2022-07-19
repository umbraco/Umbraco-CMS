// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
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

    public override IDictionary<string, object> ToValueEditor(object? configuration)
    {
        IDictionary<string, object> d = base.ToValueEditor(configuration);

        var format = d["format"].ToString()!;

        d["pickTime"] = format.ContainsAny(new[] { "H", "m", "s" });

        return d;
    }
}
