using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Services
{
    public class TwoFactorLoginService : ITwoFactorLoginService
    {
        private readonly ITwoFactorLoginRepository _twoFactorLoginRepository;
        private readonly IScopeProvider _scopeProvider;
        private readonly IDictionary<string, ITwoFactorProvider> _twoFactorSetupGenerators;

        public TwoFactorLoginService(
            ITwoFactorLoginRepository twoFactorLoginRepository,
            IScopeProvider scopeProvider,
            IEnumerable<ITwoFactorProvider> twoFactorSetupGenerators)
        {
            _twoFactorLoginRepository = twoFactorLoginRepository;
            _scopeProvider = scopeProvider;
            _twoFactorSetupGenerators = twoFactorSetupGenerators.ToDictionary(x=>x.GetType().Name);
        }

        public async Task DeleteUserLoginsAsync(Guid userOrMemberKey)
        {
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            await _twoFactorLoginRepository.DeleteUserLoginsAsync(userOrMemberKey);
        }

        public async Task<IEnumerable<string>> GetEnabledTwoFactorProviderNamesAsync(Guid userOrMemberKey)
        {
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            return (await _twoFactorLoginRepository.GetByUserOrMemberKeyAsync(userOrMemberKey))
                .Select(x => x.ProviderName).ToArray();
        }


        public async Task<bool> IsTwoFactorEnabledAsync(Guid userOrMemberKey)
        {
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            return (await _twoFactorLoginRepository.GetByUserOrMemberKeyAsync(userOrMemberKey)).Any();
        }

        public async Task<string> GetSecretForUserAndConfirmedProviderAsync(Guid userOrMemberKey, string providerName)
        {
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            return (await _twoFactorLoginRepository.GetByUserOrMemberKeyAsync(userOrMemberKey)).FirstOrDefault(x=>x.ProviderName == providerName && x.Confirmed == true)?.Secret;
        }

        public async Task<object> GetSetupInfoAsync(Guid userOrMemberKey, string providerName)
        {
            var secret = await GetSecretForUserAndConfirmedProviderAsync(userOrMemberKey, providerName);

            //Dont allow to generate a new secrets if user already has one
            if (!string.IsNullOrEmpty(secret))
            {
                return default;
            }

            secret = GenerateSecret();

            if (!_twoFactorSetupGenerators.TryGetValue(providerName, out ITwoFactorProvider generator))
            {
                throw new InvalidOperationException($"No ITwoFactorSetupGenerator found for provider: {providerName}");
            }

            return await generator.GetSetupDataAsync(userOrMemberKey, secret);
        }

        public IEnumerable<string> GetAllProviderNames() => _twoFactorSetupGenerators.Keys;
        public async Task<bool> DisableAsync(Guid userOrMemberKey, string providerName)
        {
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            return (await _twoFactorLoginRepository.DeleteUserLoginsAsync(userOrMemberKey, providerName));

        }

        public bool ValidateTwoFactorSetup(string providerName, string secret, string code)
        {
            if (!_twoFactorSetupGenerators.TryGetValue(providerName, out ITwoFactorProvider generator))
            {
                throw new InvalidOperationException($"No ITwoFactorSetupGenerator found for provider: {providerName}");
            }

            return generator.ValidateTwoFactorSetup(secret, code);
        }

        public Task SaveAsync(TwoFactorLogin twoFactorLogin)
        {
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            _twoFactorLoginRepository.Save(twoFactorLogin);

            return Task.CompletedTask;
        }


        /// <summary>
        /// Generates a new random unique secret.
        /// </summary>
        /// <returns>The random secret</returns>
        protected virtual string GenerateSecret() => Guid.NewGuid().ToString();
    }
}
