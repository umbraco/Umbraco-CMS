using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Configuration
{
    public static class ContentSettingsExtensions
    {
        /// <summary>
        /// Determines if file extension is allowed for upload based on (optional) white list and black list
        /// held in settings.
        /// Allow upload if extension is whitelisted OR if there is no whitelist and extension is NOT blacklisted.
        /// </summary>
        public static bool IsFileAllowedForUpload(this IContentSettings contentSettings, string extension)
        {
            return contentSettings.AllowedUploadFiles.Any(x => x.InvariantEquals(extension)) ||
                (contentSettings.AllowedUploadFiles.Any() == false &&
                contentSettings.DisallowedUploadFiles.Any(x => x.InvariantEquals(extension)) == false);
        }
    }
}
