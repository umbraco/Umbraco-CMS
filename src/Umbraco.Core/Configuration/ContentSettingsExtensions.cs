using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="ContentSettings" />.
/// </summary>
public static class ContentSettingsExtensions
{
    /// <summary>
    /// Determines if file extension is allowed for upload based on (optional) allow list and deny list held in settings.
    /// Disallowed file extensions are only considered if there are no allowed file extensions.
    /// </summary>
    /// <param name="contentSettings">The content settings.</param>
    /// <param name="extension">The file extension.</param>
    /// <returns>
    ///   <c>true</c> if the file extension is allowed for upload; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsFileAllowedForUpload(this ContentSettings contentSettings, string extension)
        => contentSettings.AllowedUploadedFileExtensions.Any(x => x.InvariantEquals(extension.Trim())) ||
        (contentSettings.AllowedUploadedFileExtensions.Any() == false && contentSettings.DisallowedUploadedFileExtensions.Any(x => x.InvariantEquals(extension.Trim())) == false);

    /// <summary>
    /// Determines whether the given file extension is allowed for an uploaded image, based on the configured
    /// image file types (<see cref="ContentImagingSettings.ImageFileTypes" />) and the disallowed upload
    /// extensions (<see cref="ContentSettings.DisallowedUploadedFileExtensions" />).
    /// </summary>
    /// <param name="contentSettings">The content settings.</param>
    /// <param name="extension">The file extension, without a leading period.</param>
    /// <returns>
    ///   <c>true</c> if the extension is a configured image file type and is not explicitly disallowed; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsAllowedImageFileType(this ContentSettings contentSettings, string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return false;
        }

        extension = extension.Trim();

        if (contentSettings.DisallowedUploadedFileExtensions.InvariantContains(extension))
        {
            return false;
        }

        return contentSettings.Imaging.ImageFileTypes.InvariantContains(extension);
    }

    /// <summary>
    /// Gets the auto-fill configuration for a specified property alias.
    /// </summary>
    /// <param name="contentSettings">The content settings.</param>
    /// <param name="propertyTypeAlias">The property type alias.</param>
    /// <returns>
    /// The auto-fill configuration for the specified property alias or <c>null</c> if not configured.
    /// </returns>
    public static ImagingAutoFillUploadField? GetConfig(this ContentSettings contentSettings, string propertyTypeAlias)
        => contentSettings.Imaging.AutoFillImageProperties.FirstOrDefault(x => x.Alias == propertyTypeAlias);
}
