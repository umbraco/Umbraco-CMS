using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
{
    public class ActiveDirectoryBackOfficeUserPasswordChecker : IBackOfficeUserPasswordChecker
    {
        public virtual string ActiveDirectoryDomain {
            get {
                return ConfigurationManager.AppSettings["ActiveDirectoryDomain"];
            }
        }

        public Task<BackOfficeUserPasswordCheckerResult> CheckPasswordAsync(BackOfficeIdentityUser user, string password)
        {
            bool isValid;
            using (var pc = new PrincipalContext(ContextType.Domain, ActiveDirectoryDomain))
            {
                isValid = pc.ValidateCredentials(user.UserName, password);
            }

            var result = isValid
                ? BackOfficeUserPasswordCheckerResult.ValidCredentials
                : BackOfficeUserPasswordCheckerResult.InvalidCredentials;

            return Task.FromResult(result);
        }
    }
}
