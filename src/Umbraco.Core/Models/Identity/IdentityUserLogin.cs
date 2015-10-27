using System;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models.Identity
{
    /// <summary>
    /// Entity type for a user's login (i.e. facebook, google)
    /// 
    /// </summary>
    public class IdentityUserLogin : Entity, IIdentityUserLogin
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

        /// <summary>
        /// The login provider for the login (i.e. facebook, google)
        /// 
        /// </summary>
        public string LoginProvider { get; set; }

        /// <summary>
        /// Key representing the login for the provider
        /// 
        /// </summary>
        public string ProviderKey { get; set; }

        /// <summary>
        /// User Id for the user who owns this login
        /// 
        /// </summary>
        public int UserId { get; set; }
    }
}