using System;
using System.Threading.Tasks;
using Google.Authenticator;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.UI.Security
{
    public class UmbracoAppAuthenticator : ITwoFactorProvider
    {
        public const string Name = "UmbracoAppAuthenticator";

        private readonly IMemberService _memberService;


        public UmbracoAppAuthenticator(IMemberService memberService)
        {
            _memberService = memberService;
        }

        public string ProviderName => Name;

        public async Task<object> GetSetupDataAsync(Guid userOrMemberKey, string secret)
        {
            var member = _memberService.GetByKey(userOrMemberKey);

            var twoFactorAuthenticator = new TwoFactorAuthenticator();
            SetupCode setupInfo = twoFactorAuthenticator.GenerateSetupCode("My application name", member.Username, secret, false);
            return new QrCodeSetupData()
            {
                SetupCode = setupInfo,
                Secret = secret
            };
        }

        public bool ValidateTwoFactorPIN(string secret, string code)
        {
            var twoFactorAuthenticator = new TwoFactorAuthenticator();
            return twoFactorAuthenticator.ValidateTwoFactorPIN(secret, code);
        }

        public bool ValidateTwoFactorSetup(string secret, string token) => ValidateTwoFactorPIN(secret, token);
    }
}
