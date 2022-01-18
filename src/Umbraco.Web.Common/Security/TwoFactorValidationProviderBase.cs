using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Infrastructure.Security
{
    public abstract class
        TwoFactorBackOfficeValidationProviderBase : TwoFactorValidationProviderBase<BackOfficeIdentityUser>
    {
        protected TwoFactorBackOfficeValidationProviderBase(string providerName, IDataProtectionProvider dataProtectionProvider, IOptions<DataProtectionTokenProviderOptions> options, ILogger<DataProtectorTokenProvider<BackOfficeIdentityUser>> logger, ITwoFactorLoginService twoFactorLoginService, IEnumerable<ITwoFactorSetupGenerator> twoFactorSetupGenerators) : base(providerName, dataProtectionProvider, options, logger, twoFactorLoginService, twoFactorSetupGenerators)
        {
        }

        protected override Guid GetUserKey(BackOfficeIdentityUser user) => user.Key;
    }

    public abstract class TwoFactorMemberValidationProviderBase : TwoFactorValidationProviderBase<MemberIdentityUser>
    {
        protected TwoFactorMemberValidationProviderBase(string providerName, IDataProtectionProvider dataProtectionProvider, IOptions<DataProtectionTokenProviderOptions> options, ILogger<DataProtectorTokenProvider<MemberIdentityUser>> logger, ITwoFactorLoginService twoFactorLoginService, IEnumerable<ITwoFactorSetupGenerator> twoFactorSetupGenerators) : base(providerName, dataProtectionProvider, options, logger, twoFactorLoginService, twoFactorSetupGenerators)
        {
        }

        protected override Guid GetUserKey(MemberIdentityUser user) => user.Key;
    }

    public abstract class TwoFactorValidationProviderBase<TUmbracoIdentityUser>
        : DataProtectorTokenProvider<TUmbracoIdentityUser>
        where TUmbracoIdentityUser : UmbracoIdentityUser
    {
        private readonly string _providerName;
        private readonly ITwoFactorLoginService _twoFactorLoginService;
        private readonly ITwoFactorSetupGenerator _generator;

        protected TwoFactorValidationProviderBase(
            string providerName,
            IDataProtectionProvider dataProtectionProvider,
            IOptions<DataProtectionTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<TUmbracoIdentityUser>> logger,
            ITwoFactorLoginService twoFactorLoginService,
            IEnumerable<ITwoFactorSetupGenerator> twoFactorSetupGenerators)
            : base(dataProtectionProvider, options, logger)
        {
            _providerName = providerName;
            _twoFactorLoginService = twoFactorLoginService;
            _generator = twoFactorSetupGenerators.SingleOrDefault(x => x.ProviderName == providerName);
        }

        public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUmbracoIdentityUser> manager,
            TUmbracoIdentityUser user) => Task.FromResult(true);

        public override async Task<bool> ValidateAsync(string purpose, string token,
            UserManager<TUmbracoIdentityUser> manager, TUmbracoIdentityUser user)
        {
            var secret =
                await _twoFactorLoginService.GetSecretForUserAndConfirmedProviderAsync(GetUserKey(user), _providerName);

            if (secret is null)
            {
                return false;
            }

            var validToken = _generator.ValidateTwoFactorPIN(secret, token);


            return validToken;
        }

        protected abstract Guid GetUserKey(TUmbracoIdentityUser user);

    }
}
