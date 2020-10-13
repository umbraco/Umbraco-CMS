using System;
using System.Security;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models.Membership;
using Microsoft.Owin;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models;

namespace Umbraco.Web.Security
{

    // NOTE: Moved to netcore
    public class BackofficeSecurity : IBackofficeSecurity
    {
        public IUser CurrentUser => throw new NotImplementedException();

        public ValidateRequestAttempt AuthorizeRequest(bool throwExceptions = false)
        {
            throw new NotImplementedException();
        }

        public Attempt<int> GetUserId()
        {
            throw new NotImplementedException();
        }

        public bool IsAuthenticated()
        {
            throw new NotImplementedException();
        }

        public bool UserHasSectionAccess(string section, IUser user)
        {
            throw new NotImplementedException();
        }

        public bool ValidateCurrentUser()
        {
            throw new NotImplementedException();
        }

        public ValidateRequestAttempt ValidateCurrentUser(bool throwExceptions, bool requiresApproval = true)
        {
            throw new NotImplementedException();
        }
    }
}
