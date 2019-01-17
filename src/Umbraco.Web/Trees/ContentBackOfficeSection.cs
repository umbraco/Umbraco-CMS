using Umbraco.Core;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Defines the back office content section
    /// </summary>
    public class ContentBackOfficeSection : IBackOfficeSection
    {
        public string Alias => Constants.Applications.Content;
        public string Name => "Content";
    }
}
