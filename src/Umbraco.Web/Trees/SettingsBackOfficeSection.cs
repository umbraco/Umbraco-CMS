using Umbraco.Core;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Defines the back office settings section
    /// </summary>
    public class SettingsBackOfficeSection : IBackOfficeSection
    {
        public string Alias => Constants.Applications.Settings;
        public string Name => "Settings";
    }
}