namespace Umbraco.Core.Dashboards
{
    public interface IContentDashboardSettings
    {
        /// <summary>
        /// Gets a value indicating whether the content dashboard should be available to all users.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the dashboard is visible for all user groups; otherwise, <c>false</c>
        ///     and the default access rules for that dashboard will be in use.
        /// </value>
        bool AllowContentDashboardAccessToAllUsers { get; }

        /// <summary>
        /// Gets the path to use when constructing the URL for retrieving data for the content dashboard.
        /// </summary>
        /// <value>The URL path.</value>
        string ContentDashboardPath { get; }

        /// <summary>
        /// Gets the allowed addresses to retrieve data for the content dashboard.
        /// </summary>
        /// <value>The URLs.</value>
        string ContentDashboardUrlAllowlist { get; }
    }
}
