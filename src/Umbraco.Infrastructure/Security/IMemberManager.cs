using Umbraco.Core.Security;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// The user manager for members
    /// </summary>
    public interface IMemberManager : IUmbracoUserManager<MembersIdentityUser>
    {
    }
}
