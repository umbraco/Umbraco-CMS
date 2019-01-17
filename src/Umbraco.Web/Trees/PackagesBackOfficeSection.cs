using Umbraco.Core;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Defines the back office packages section
    /// </summary>
    public class PackagesBackOfficeSection : IBackOfficeSection
    {
        public string Alias => Constants.Applications.Packages;
        public string Name => "Packages";
    }
}