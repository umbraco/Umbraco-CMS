using Umbraco.Core;
using Umbraco.Core.Models.Sections;

namespace Umbraco.Web.Sections
{
    /// <summary>
    /// Defines the back office users section
    /// </summary>
    public class UsersSection : ISection
    {
        /// <inheritdoc />
        public string Alias => Constants.Applications.Users;

        /// <inheritdoc />
        public string Name => "Users";
    }
}
