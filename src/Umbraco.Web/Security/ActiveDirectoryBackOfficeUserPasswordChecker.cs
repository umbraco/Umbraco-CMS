using System;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Web.Security
{
    // TODO: This relies on an assembly that is not .NET Standard (at least not at the time of implementation) :(
    // TODO: This could be ported now, see https://stackoverflow.com/questions/37330705/working-with-directoryservices-in-asp-net-core
    public class ActiveDirectoryBackOfficeUserPasswordChecker : IBackOfficeUserPasswordChecker
    {
        private readonly IOptions<ActiveDirectorySettings> _activeDirectorySettings;

        public ActiveDirectoryBackOfficeUserPasswordChecker(IOptions<ActiveDirectorySettings> activeDirectorySettings)
        {
            _activeDirectorySettings = activeDirectorySettings;
        }

        public virtual string ActiveDirectoryDomain => _activeDirectorySettings.Value.Domain;

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
