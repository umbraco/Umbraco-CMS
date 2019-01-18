using Umbraco.Core;
using Umbraco.Core.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Defines the back office users section
    /// </summary>
    public class UsersBackOfficeSection : IBackOfficeSection
    {
        /// <inheritdoc />
        public string Alias => Constants.Applications.Users;

        /// <inheritdoc />
        public string Name => "Users";
    }
}
