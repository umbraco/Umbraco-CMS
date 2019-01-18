using Umbraco.Core;
using Umbraco.Core.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Defines the back office members section
    /// </summary>
    public class MembersBackOfficeSection : IBackOfficeSection
    {
        /// <inheritdoc />
        public string Alias => Constants.Applications.Members;

        /// <inheritdoc />
        public string Name => "Members";
    }
}
