using Umbraco.Core;
using Umbraco.Core.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Defines the back office media section
    /// </summary>
    public class MediaBackOfficeSection : IBackOfficeSection
    {
        public string Alias => Constants.Applications.Media;

        public string Name => "Media";

        public int SortOrder => 20;
    }
}
