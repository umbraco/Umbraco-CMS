using System;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.Security
{
    // TODO: This relies on an assembly that is not .NET Standard (at least not at the time of implementation) :(
    public class ActiveDirectoryBackOfficeUserPasswordChecker : IBackOfficeUserPasswordChecker
    {
        private readonly IActiveDirectorySettings _settings;

        public ActiveDirectoryBackOfficeUserPasswordChecker(IActiveDirectorySettings settings)
        {
            _settings = settings;
        }

        public virtual string ActiveDirectoryDomain => _settings.ActiveDirectoryDomain;

        public Task<BackOfficeUserPasswordCheckerResult> CheckPasswordAsync(BackOfficeIdentityUser user, string password)
        {
            bool isValid;
            using (var pc = new PrincipalContext(ContextType.Domain, ActiveDirectoryDomain))
            {
                isValid = pc.ValidateCredentials(user.UserName, password);
            }

            if (isValid && user.HasIdentity == false)
            {
                // TODO: the user will need to be created locally (i.e. auto-linked)
                throw new NotImplementedException("The user " + user.UserName + " does not exist locally and currently the " + typeof(ActiveDirectoryBackOfficeUserPasswordChecker) + " doesn't support auto-linking, see http://issues.umbraco.org/issue/U4-10181");
            }

            var result = isValid
                ? BackOfficeUserPasswordCheckerResult.ValidCredentials
                : BackOfficeUserPasswordCheckerResult.InvalidCredentials;

            return Task.FromResult(result);
        }
    }
}
