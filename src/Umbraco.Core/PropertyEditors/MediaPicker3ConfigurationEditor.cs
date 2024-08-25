using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the media picker value editor.
/// </summary>
public class MediaPicker3ConfigurationEditor : ConfigurationEditor<MediaPicker3Configuration>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaPicker3ConfigurationEditor" /> class.
    /// </summary>
    public MediaPicker3ConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
