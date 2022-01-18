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
        private readonly IDictionary<string, ITwoFactorSetupGenerator> _twoFactorSetupGenerators;

        public TwoFactorLoginService(
            ITwoFactorLoginRepository twoFactorLoginRepository,
            IScopeProvider scopeProvider,
            IEnumerable<ITwoFactorSetupGenerator> twoFactorSetupGenerators)
        {
            _twoFactorLoginRepository = twoFactorLoginRepository;
            _scopeProvider = scopeProvider;
            _twoFactorSetupGenerators = twoFactorSetupGenerators.ToDictionary(x=>x.ProviderName);
        }

        public async Task DeleteUserLoginsAsync(Guid userOrMemberKey)
        {
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            await _twoFactorLoginRepository.DeleteUserLoginsAsync(userOrMemberKey);
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

        public async Task<TwoFactorLoginSetupInfo> GetSetupInfoAsync(Guid userOrMemberKey, string providerName)
        {
            var secret = await GetSecretForUserAndConfirmedProviderAsync(userOrMemberKey, providerName);

            //Dont allow to generate a new secrets if user already has one
            if (!string.IsNullOrEmpty(secret))
            {
                return null;
            }

            secret = GenerateSecret();

            if (!_twoFactorSetupGenerators.TryGetValue(providerName, out ITwoFactorSetupGenerator generator))
            {
                throw new InvalidOperationException($"No ITwoFactorSetupGenerator found for provider: {providerName}");
            }


            var qrCodeUrl = await generator.GetSetupQrCodeUrlAsync(userOrMemberKey, secret);

            return new TwoFactorLoginSetupInfo(secret, qrCodeUrl);
        }

        public IEnumerable<string> GetAllProviderNames() => _twoFactorSetupGenerators.Keys;
        public async Task<bool> DisableAsync(Guid userOrMemberKey, string providerName)
        {
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            return (await _twoFactorLoginRepository.DeleteUserLoginsAsync(userOrMemberKey, providerName));

        }

        public bool ValidateTwoFactorPIN(string providerName, string secret, string code)
        {
            if (!_twoFactorSetupGenerators.TryGetValue(providerName, out ITwoFactorSetupGenerator generator))
            {
                throw new InvalidOperationException($"No ITwoFactorSetupGenerator found for provider: {providerName}");
            }

            return generator.ValidateTwoFactorPIN(secret, code);
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
