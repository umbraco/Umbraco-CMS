using Umbraco.Core;
using Umbraco.Core.Models.Sections;

namespace Umbraco.Web.Sections
{
    /// <summary>
    /// Defines the back office packages section
    /// </summary>
    public class PackagesSection : ISection
    {
        /// <inheritdoc />
        public string Alias => Constants.Applications.Packages;

        /// <inheritdoc />
        public string Name => "Packages";
    }
}
