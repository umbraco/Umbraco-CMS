using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Composing;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Security;

namespace Umbraco.Web.Common.Security
{
    // TODO: need to implement this

    public class WebSecurity : IWebSecurity
    {
        public IUser CurrentUser => new User(Current.Configs.Global());

        public ValidateRequestAttempt AuthorizeRequest(bool throwExceptions = false)
        {
            return ValidateRequestAttempt.Success;
        }

        public void ClearCurrentLogin()
        {
            //throw new NotImplementedException();
        }

        public Attempt<int> GetUserId()
        {
            return Attempt.Succeed(-1);
        }

        public bool IsAuthenticated()
        {
            return true;
        }

        public double PerformLogin(int userId)
        {
            return 100;
        }

        public bool UserHasSectionAccess(string section, IUser user)
        {
            return true;
        }

        public bool ValidateCurrentUser()
        {
            return true;
        }

        public ValidateRequestAttempt ValidateCurrentUser(bool throwExceptions, bool requiresApproval = true)
        {
            return ValidateRequestAttempt.Success;
        }
    }
}
