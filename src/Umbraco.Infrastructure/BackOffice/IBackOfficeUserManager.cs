using Umbraco.Core.Security;

namespace Umbraco.Core.BackOffice
{
    /// <summary>
    /// The user manager for the back office
    /// </summary>
    public interface IBackOfficeUserManager : IUmbracoUserManager<BackOfficeIdentityUser>
    {
    }
}
