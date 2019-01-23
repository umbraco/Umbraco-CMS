namespace Umbraco.Core.Dashboards
{
    public interface IDashboardSection
    {
        /// <summary>
        /// Display name of the dashboard tab
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Alias to refer to this dashboard via code
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// A collection of sections/application aliases that this dashboard will appear on
        /// </summary>
        string[] Sections { get; }

        /// <summary>
        /// The HTML view to load for the dashboard
        /// </summary>
        string View { get; }

        /// <summary>
        /// Dashboards can be shown/hidden based on access rights
        /// </summary>
        IAccessRule[] AccessRules { get; }
    }
}
