// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the image cropper value editor.
/// </summary>
internal class ImageCropperConfigurationEditor : ConfigurationEditor<ImageCropperConfiguration>
{
    public ImageCropperConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }

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
