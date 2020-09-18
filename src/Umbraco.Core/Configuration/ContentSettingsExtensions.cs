using System;
using System.Linq;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Configuration
{
    public static class ContentSettingsExtensions
    {
        /// <summary>
        /// Gets a value indicating whether the file extension corresponds to an image.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <param name="contentConfig"></param>
        /// <returns>A value indicating whether the file extension corresponds to an image.</returns>
        public static bool IsImageFile(this ContentSettings contentConfig, string extension)
        {
            if (contentConfig == null) throw new ArgumentNullException(nameof(contentConfig));
            if (extension == null) return false;
            extension = extension.TrimStart('.');
            return contentConfig.Imaging.ImageFileTypes.InvariantContains(extension);
        }

        /// <summary>
        /// Determines if file extension is allowed for upload based on (optional) white list and black list
        /// held in settings.
        /// Allow upload if extension is whitelisted OR if there is no whitelist and extension is NOT blacklisted.
        /// </summary>
        public static bool IsFileAllowedForUpload(this ContentSettings contentSettings, string extension)
        {
            return contentSettings.AllowedUploadFiles.Any(x => x.InvariantEquals(extension)) ||
                (contentSettings.AllowedUploadFiles.Any() == false &&
                contentSettings.DisallowedUploadFiles.Any(x => x.InvariantEquals(extension)) == false);
        }

        /// <summary>
        /// Gets the auto-fill configuration for a specified property alias.
        /// </summary>
        /// <param name="contentSettings"></param>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <returns>The auto-fill configuration for the specified property alias, or null.</returns>
        public static ImagingAutoFillUploadField GetConfig(this ContentSettings contentSettings, string propertyTypeAlias)
        {
            var autoFillConfigs = contentSettings.Imaging.AutoFillImageProperties;
            return autoFillConfigs?.FirstOrDefault(x => x.Alias == propertyTypeAlias);
        }
    }
}
