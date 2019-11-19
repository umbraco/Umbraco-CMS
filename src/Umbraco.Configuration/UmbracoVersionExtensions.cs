using System.Configuration;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration
{
    public static class UmbracoVersionExtensions
    {
        /// <summary>
        /// Gets the "local" version of the site.
        /// </summary>
        /// <remarks>
        /// <para>Three things have a version, really: the executing code, the database model,
        /// and the site/files. The database model version is entirely managed via migrations,
        /// and changes during an upgrade. The executing code version changes when new code is
        /// deployed. The site/files version changes during an upgrade.</para>
        /// </remarks>
        public static SemVersion LocalVersion(this IUmbracoVersion umbracoVersion)
        {
            try
            {
                // TODO: https://github.com/umbraco/Umbraco-CMS/issues/4238 - stop having version in web.config appSettings
                var value = ConfigurationManager.AppSettings[Constants.AppSettings.ConfigurationStatus];
                return value.IsNullOrWhiteSpace() ? null : SemVersion.TryParse(value, out var semver) ? semver : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
