using System;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.UI.Security
{
    public class EmailTwoFactorMemberProvider : ITwoFactorProvider
    {
        public const string Name = "EmailTwoFactorMemberProvider";
        public string ProviderName => Name;
        public async Task<object> GetSetupDataAsync(Guid userOrMemberKey, string secret)
        {
            //Generate code and save on user
            //send email with code, using the email from the ValidateTwoFactorSetup

            return secret;
        }
        public bool ValidateTwoFactorPIN(string secret, string code)
        {
            //Verify the code is equal to the one we generated in GetSetupDataAsync

            return code.Equals("111111");
        }

        public bool ValidateTwoFactorSetup(string secret, string token)
        {
            // Verify the secret is correct and use the token. e.g. save an email/phone number to send messages to

            return true;
        }

    }
}
