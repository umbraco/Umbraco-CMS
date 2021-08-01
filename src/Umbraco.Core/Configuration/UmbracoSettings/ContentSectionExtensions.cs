using System;
using System.Linq;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public static class ContentSectionExtensions
    {
        /// <summary>
        /// Gets a value indicating whether the file extension corresponds to an image.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <param name="contentConfig"></param>
        /// <returns>A value indicating whether the file extension corresponds to an image.</returns>
        public static bool IsImageFile(this IContentSection contentConfig, string extension)
        {
            if (contentConfig == null) throw new ArgumentNullException(nameof(contentConfig));
            if (extension == null) return false;
            extension = extension.TrimStart(Constants.CharArrays.Period);
            return contentConfig.ImageFileTypes.InvariantContains(extension);
        }

        /// <summary>
        /// Determines if file extension is allowed for upload based on (optional) white list and black list
        /// held in settings.
        /// Allow upload if extension is whitelisted OR if there is no whitelist and extension is NOT blacklisted.
        /// </summary>
        public static bool IsFileAllowedForUpload(this IContentSection contentSection, string extension)
        {
            return contentSection.AllowedUploadFiles.Any(x => x.InvariantEquals(extension)) ||
                (contentSection.AllowedUploadFiles.Any() == false &&
                contentSection.DisallowedUploadFiles.Any(x => x.InvariantEquals(extension)) == false);
        }

        /// <summary>
        /// Gets the auto-fill configuration for a specified property alias.
        /// </summary>
        /// <param name="contentSection"></param>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <returns>The auto-fill configuration for the specified property alias, or null.</returns>
        public static IImagingAutoFillUploadField GetConfig(this IContentSection contentSection, string propertyTypeAlias)
        {
            var autoFillConfigs = contentSection.ImageAutoFillProperties;
            return autoFillConfigs?.FirstOrDefault(x => x.Alias == propertyTypeAlias);
        }
    }
}
