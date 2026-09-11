using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for configuring and working with media picker settings in Umbraco.
/// </summary>
/// <remarks>
/// Kept so that code compiled against this class keeps working; the media picker configuration is now shared by
/// more than one editor, and <see cref="MediaPickerConfigurationExtensions" /> is named for the family.
/// </remarks>
[Obsolete("Use MediaPickerConfigurationExtensions instead. Scheduled for removal in Umbraco 21.")]
public static class MediaPicker3ConfigurationExtensions
{
    /// <summary>
    ///     Applies the configuration to ensure only valid crops are kept and have the correct width/height.
    /// </summary>
    public static void ApplyConfiguration(this ImageCropperValue imageCropperValue, MediaPicker3Configuration? configuration)
        => MediaPickerConfigurationExtensions.ApplyConfiguration(imageCropperValue, configuration);
}
