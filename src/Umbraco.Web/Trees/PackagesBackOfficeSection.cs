using Umbraco.Core;
using Umbraco.Core.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Defines the back office packages section
    /// </summary>
    public class PackagesBackOfficeSection : IBackOfficeSection
    {
        /// <inheritdoc />
        public string Alias => Constants.Applications.Packages;

        /// <inheritdoc />
        public string Name => "Packages";

        /// <inheritdoc />
        public int SortOrder => 40;
    }
}
