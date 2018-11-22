using System.Threading.Tasks;
using System.Web.Security;
using Microsoft.AspNet.Identity;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// Ensure that both the normal password validator rules are processed along with the underlying memberhsip provider rules
    /// </summary>
    public class MembershipProviderPasswordValidator : PasswordValidator
    {
        public MembershipProvider Provider { get; private set; }

        public MembershipProviderPasswordValidator(MembershipProvider provider)
        {
            Provider = provider;

            RequiredLength = Provider.MinRequiredPasswordLength;
            RequireNonLetterOrDigit = Provider.MinRequiredNonAlphanumericCharacters > 0;
            RequireDigit = false;
            RequireLowercase = false;
            RequireUppercase = false;
        }

        public override async Task<IdentityResult> ValidateAsync(string item)
        {
            var result = await base.ValidateAsync(item);
            if (result.Succeeded == false)
                return result;
            var providerValidate = MembershipProviderBase.IsPasswordValid(item, Provider.MinRequiredNonAlphanumericCharacters, Provider.PasswordStrengthRegularExpression, Provider.MinRequiredPasswordLength);
            if (providerValidate.Success == false)
            {
                return IdentityResult.Failed("Could not set password, password rules violated: " + providerValidate.Result);
            }
            return IdentityResult.Success;
        }
    }
}