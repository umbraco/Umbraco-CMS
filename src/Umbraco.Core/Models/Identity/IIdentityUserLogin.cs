using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models.Identity
{
    public interface IIdentityUserLogin : IEntity, IRememberBeingDirty
    {
        /// <summary>
        /// The login provider for the login (i.e. facebook, google)
        ///
        /// </summary>
        string LoginProvider { get; set; }

        /// <summary>
        /// Key representing the login for the provider
        ///
        /// </summary>
        string ProviderKey { get; set; }

        /// <summary>
        /// User Id for the user who owns this login
        ///
        /// </summary>
        int UserId { get; set; }
    }
}
