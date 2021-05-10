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
        /// Gets the base URL for content on the content dashboard when running on Umbraco Cloud.
        /// </summary>
        /// <value>The base URL.</value>
        string ContentDashboardBaseUrl { get; }
    }
}
