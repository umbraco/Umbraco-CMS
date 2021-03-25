using System.Configuration;

namespace Umbraco.Core.Dashboards
{
    public class ContentDashboardSettings: IContentDashboardSettings
    {

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
    }
}
