using Umbraco.Core;
using Umbraco.Core.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Defines the back office content section
    /// </summary>
    public class ContentBackOfficeSection : IBackOfficeSection
    {
        /// <inheritdoc />
        public string Alias => Constants.Applications.Content;

        /// <inheritdoc />
        public string Name => "Content";

        public int SortOrder => 10;
    }
}
