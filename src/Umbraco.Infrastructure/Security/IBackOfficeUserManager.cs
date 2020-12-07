using Umbraco.Core.Security;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// The user manager for the back office
    /// </summary>
    public interface IBackOfficeUserManager : IUmbracoUserManager<BackOfficeIdentityUser>
    {
    }
}
