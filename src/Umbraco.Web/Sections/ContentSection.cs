using Umbraco.Core;
using Umbraco.Core.Models.Sections;

namespace Umbraco.Web.Sections
{
    /// <summary>
    /// Defines the back office content section
    /// </summary>
    public class ContentSection : ISection
    {
        /// <inheritdoc />
        public string Alias => Constants.Applications.Content;

        /// <inheritdoc />
        public string Name => "Content";
    }
}
