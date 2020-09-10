using System;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models.Identity
{

    /// <summary>
    /// Entity type for a user's login (i.e. Facebook, Google)
    /// </summary>
    public class IdentityUserLogin : EntityBase, IIdentityUserLoginExtended
    {
        public IdentityUserLogin(string loginProvider, string providerKey, int userId)
        {
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
            UserId = userId;
        }

        public IdentityUserLogin(int id, string loginProvider, string providerKey, int userId, DateTime createDate)
        {
            Id = id;
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
            UserId = userId;
            CreateDate = createDate;
        }

        /// <inheritdoc />
        public string LoginProvider { get; set; }

        /// <inheritdoc />
        public string ProviderKey { get; set; }

        /// <inheritdoc />
        public int UserId { get; set; }

        /// <inheritdoc />
        public string UserData { get; set; }
    }
}
