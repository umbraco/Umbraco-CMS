using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models.Identity
{
    // TODO: Merge these in v8! This is here purely for backward compat

    public interface IIdentityUserLoginExtended : IIdentityUserLogin
    {
        /// <summary>
        /// Used to store any arbitrary data for the user and external provider - like user tokens returned from the provider
        /// </summary>
        string UserData { get; set; }
    }

    public interface IIdentityUserLogin : IEntity, IRememberBeingDirty
    {
        /// <summary>
        /// The login provider for the login (i.e. Facebook, Google)
        /// </summary>
        string LoginProvider { get; set; }

        /// <summary>
        /// Key representing the login for the provider
        /// </summary>
        string ProviderKey { get; set; }

        /// <summary>
        /// User Id for the user who owns this login
        /// </summary>
        int UserId { get; set; }
    }
}
