using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Extensions;

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
        => contentSettings.AllowedUploadedFileExtensions.Any(x => x.InvariantEquals(extension)) ||
        (contentSettings.AllowedUploadedFileExtensions.Any() == false && contentSettings.DisallowedUploadedFileExtensions.Any(x => x.InvariantEquals(extension)) == false);

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
