using System.Configuration;

namespace Umbraco.Core.Dashboards
{
    public class ContentDashboardSettings : IContentDashboardSettings
    {
        private const string DefaultContentDashboardPath = "cms";

        /// <summary>
        /// Gets a value indicating whether the content dashboard should be available to all users.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the dashboard is visible for all user groups; otherwise, <c>false</c>
        ///     and the default access rules for that dashboard will be in use.
        /// </value>
        public bool AllowContentDashboardAccessToAllUsers
        {
            get
            {
                bool.TryParse(ConfigurationManager.AppSettings[Constants.AppSettings.AllowContentDashboardAccessToAllUsers], out var value);
                return value;
            }
        }

        /// <summary>
        /// Gets the path to use when constructing the URL for retrieving data for the content dashboard.
        /// </summary>
        /// <value>The URL path.</value>
        public string ContentDashboardPath =>
            ConfigurationManager.AppSettings.ContainsKey(Constants.AppSettings.ContentDashboardPath)
                ? ConfigurationManager.AppSettings[Constants.AppSettings.ContentDashboardPath]
                : DefaultContentDashboardPath;

        /// <summary>
        /// Gets the allowed addresses to retrieve data for the content dashboard.
        /// </summary>
        /// <value>The URLs.</value>
        public string ContentDashboardUrlAllowlist =>
            ConfigurationManager.AppSettings.ContainsKey(Constants.AppSettings.ContentDashboardUrlAllowlist)
                ? ConfigurationManager.AppSettings[Constants.AppSettings.ContentDashboardUrlAllowlist]
                : null;
    }
}
