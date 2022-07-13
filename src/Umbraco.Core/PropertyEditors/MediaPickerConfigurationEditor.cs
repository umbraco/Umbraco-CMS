// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the media picker value editor.
/// </summary>
public class MediaPickerConfigurationEditor : ConfigurationEditor<MediaPickerConfiguration>
{
    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public MediaPickerConfigurationEditor(IIOHelper ioHelper)
        : this(ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaPickerConfigurationEditor" /> class.
    /// </summary>
    public MediaPickerConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser) =>

        // configure fields
        // this is not part of ContentPickerConfiguration,
        // but is required to configure the UI editor (when editing the configuration)
        Field(nameof(MediaPickerConfiguration.StartNodeId))
            .Config = new Dictionary<string, object> { { "idType", "udi" } };

    public override IDictionary<string, object> ToValueEditor(object? configuration)
    {
        // get the configuration fields
        IDictionary<string, object> d = base.ToValueEditor(configuration);

        // add extra fields
        // not part of ContentPickerConfiguration but used to configure the UI editor
        d["idType"] = "udi";

        return d;
    }
}
