using Umbraco.Core;
using Umbraco.Core.Models.Sections;

namespace Umbraco.Web.Sections
{
    /// <summary>
    /// Defines the back office members section
    /// </summary>
    public class MembersSection : ISection
    {
        /// <inheritdoc />
        public string Alias => ConstantsCore.Applications.Members;

        /// <inheritdoc />
        public string Name => "Members";
    }
}
