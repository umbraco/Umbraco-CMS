using System.Linq;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public static class ContentSectionExtensions
    {
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
    }
}