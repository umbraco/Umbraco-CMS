using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services
{
    public interface ITwoFactorLoginService : IService
    {
        /// <summary>
        /// Deletes all user logins - normally used when a member is deleted
        /// </summary>
        Task DeleteUserLoginsAsync(Guid userOrMemberKey);

        Task<bool> IsTwoFactorEnabledAsync(Guid userKey);
        Task<string> GetSecretForUserAndProviderAsync(Guid userKey, string providerName);

        Task<object> GetSetupInfoAsync(Guid userOrMemberKey, string providerName);

        IEnumerable<string> GetAllProviderNames();
        Task<bool> DisableAsync(Guid userOrMemberKey, string providerName);

        bool ValidateTwoFactorSetup(string providerName, string secret, string code);
        Task SaveAsync(TwoFactorLogin twoFactorLogin);
        Task<IEnumerable<string>> GetEnabledTwoFactorProviderNamesAsync(Guid userOrMemberKey);
    }

}
