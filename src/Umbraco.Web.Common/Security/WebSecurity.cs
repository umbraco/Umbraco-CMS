using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Security;

namespace Umbraco.Web.Common.Security
{
    // TODO: need to implement this

    public class WebSecurity : IWebSecurity
    {
        public IUser CurrentUser => throw new NotImplementedException();

        public ValidateRequestAttempt AuthorizeRequest(bool throwExceptions = false)
        {
            throw new NotImplementedException();
        }

        public void ClearCurrentLogin()
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

        public double PerformLogin(int userId)
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
