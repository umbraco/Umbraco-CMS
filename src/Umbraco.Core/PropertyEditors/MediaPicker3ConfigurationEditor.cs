using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the media picker value editor.
/// </summary>
public class MediaPicker3ConfigurationEditor : ConfigurationEditor<MediaPicker3Configuration>
{
    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public MediaPicker3ConfigurationEditor(IIOHelper ioHelper)
        : this(ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaPicker3ConfigurationEditor" /> class.
    /// </summary>
    public MediaPicker3ConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
        // configure fields
        // this is not part of ContentPickerConfiguration,
        // but is required to configure the UI editor (when editing the configuration)
        Field(nameof(MediaPicker3Configuration.StartNodeId))
            .Config = new Dictionary<string, object> { { "idType", "udi" } };

        Field(nameof(MediaPicker3Configuration.Filter))
            .Config = new Dictionary<string, object> { { "itemType", "media" } };
    }
}
