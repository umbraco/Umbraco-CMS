using System;
using System.Security;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models.Membership;
using Microsoft.AspNetCore.Http;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models;
using Umbraco.Web.Common.Extensions;
using Umbraco.Web.Security;

namespace Umbraco.Web.Common.Security
{

    /// <summary>
    /// A utility class used for dealing with USER security in Umbraco
    /// </summary>
    public class WebSecurity : IWebSecurity
    {
        private IUser _currentUser;


        public IUser CurrentUser
        {
            get => _currentUser;
            set => _currentUser = value;
        }

        public double PerformLogin(int userId)
        {
            return 15;
        }

        public void ClearCurrentLogin()
        {

        }

        public Attempt<int> GetUserId()
        {
            return new Attempt<int>();
        }

        public bool ValidateCurrentUser()
        {
            return false;
        }

        public ValidateRequestAttempt ValidateCurrentUser(bool throwExceptions, bool requiresApproval = true) => throw new NotImplementedException();

        public ValidateRequestAttempt AuthorizeRequest(bool throwExceptions = false) => throw new NotImplementedException();

        public bool UserHasSectionAccess(string section, IUser user) => false;

        public bool IsAuthenticated() => false;
    }
}
