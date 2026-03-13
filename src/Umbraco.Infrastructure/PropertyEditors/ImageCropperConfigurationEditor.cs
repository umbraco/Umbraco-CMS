// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the image cropper value editor.
/// </summary>
internal sealed class ImageCropperConfigurationEditor : ConfigurationEditor<ImageCropperConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageCropperConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">An <see cref="IIOHelper"/> instance used for file system operations.</param>
    public ImageCropperConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }

    /// <summary>
    /// Converts the provided configuration dictionary into a value editor configuration for the image cropper.
    /// Ensures that the required keys "focalPoint" and "src" are present, adding default values if they are missing.
    /// </summary>
    /// <param name="configuration">The initial configuration dictionary to convert.</param>
    /// <returns>
    /// A dictionary representing the value editor configuration, guaranteed to contain the keys "focalPoint" (with default { left = 0.5, top = 0.5 }) and "src" (with default empty string) if they were not already present.
    /// </returns>
    public override IDictionary<string, object> ToValueEditor(IDictionary<string, object> configuration)
    {
        IDictionary<string, object> config = base.ToValueEditor(configuration);
        if (!config.ContainsKey("focalPoint"))
        {
            config["focalPoint"] = new { left = 0.5, top = 0.5 };
        }

        if (!config.ContainsKey("src"))
        {
            config["src"] = string.Empty;
        }

        return config;
    }
}
