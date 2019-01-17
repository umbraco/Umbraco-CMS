using Umbraco.Core;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Defines the back office users section
    /// </summary>
    public class UsersBackOfficeSection : IBackOfficeSection
    {
        public string Alias => Constants.Applications.Users;
        public string Name => "Users";
    }
}