using Umbraco.Core;
using Umbraco.Core.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Defines the back office settings section
    /// </summary>
    public class SettingsBackOfficeSection : IBackOfficeSection
    {
        /// <inheritdoc />
        public string Alias => Constants.Applications.Settings;

        /// <inheritdoc />
        public string Name => "Settings";

        /// <inheritdoc />
        public int SortOrder => 30;
    }
}
