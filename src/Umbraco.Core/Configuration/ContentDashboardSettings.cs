
namespace Umbraco.Core.Dashboards
{
    public class ContentDashboardSettings
    {
        private const string DefaultContentDashboardPath = "cms";
        
        /// <summary>
        /// Gets a value indicating whether the content dashboard should be available to all users.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the dashboard is visible for all user groups; otherwise, <c>false</c>
        ///     and the default access rules for that dashboard will be in use.
        /// </value>
        public bool AllowContentDashboardAccessToAllUsers { get; set; } = true;

        /// <summary>
        /// Gets the path to use when constructing the URL for retrieving data for the content dashboard.
        /// </summary>
        /// <value>The URL path.</value>
        public string ContentDashboardPath  { get; set; } = DefaultContentDashboardPath;
    }
}
