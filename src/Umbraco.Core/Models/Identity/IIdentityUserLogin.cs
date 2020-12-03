using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models.Identity
{
    /// <summary>
    /// An external login provider linked to a user
    /// </summary>
    /// <typeparam name="TKey">The PK type for the user</typeparam>
    public interface IIdentityUserLogin : IEntity, IRememberBeingDirty
    {
        /// <summary>
        /// Gets or sets the login provider for the login (i.e. Facebook, Google)
        /// </summary>
        string LoginProvider { get; set; }

        /// <summary>
        /// Gets or sets key representing the login for the provider
        /// </summary>
        string ProviderKey { get; set; }

        /// <summary>
        /// Gets or sets user Id for the user who owns this login
        /// </summary>
        int UserId { get; set; }

        /// <summary>
        /// Gets or sets any arbitrary data for the user and external provider - like user tokens returned from the provider
        /// </summary>
        string UserData { get; set; }
    }
}
