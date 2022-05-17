using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Extensions;

public static class ContentSettingsExtensions
{
    /// <summary>
    ///     Determines if file extension is allowed for upload based on (optional) white list and black list
    ///     held in settings.
    ///     Allow upload if extension is whitelisted OR if there is no whitelist and extension is NOT blacklisted.
    /// </summary>
    public static bool IsFileAllowedForUpload(this ContentSettings contentSettings, string extension) =>
        contentSettings.AllowedUploadFiles.Any(x => x.InvariantEquals(extension)) ||
        (contentSettings.AllowedUploadFiles.Any() == false &&
         contentSettings.DisallowedUploadFiles.Any(x => x.InvariantEquals(extension)) == false);

    /// <summary>
    ///     Gets the auto-fill configuration for a specified property alias.
    /// </summary>
    /// <param name="contentSettings"></param>
    /// <param name="propertyTypeAlias">The property type alias.</param>
    /// <returns>The auto-fill configuration for the specified property alias, or null.</returns>
    public static ImagingAutoFillUploadField? GetConfig(this ContentSettings contentSettings, string propertyTypeAlias)
    {
        ImagingAutoFillUploadField[] autoFillConfigs = contentSettings.Imaging.AutoFillImageProperties;
        return autoFillConfigs?.FirstOrDefault(x => x.Alias == propertyTypeAlias);
    }
}
