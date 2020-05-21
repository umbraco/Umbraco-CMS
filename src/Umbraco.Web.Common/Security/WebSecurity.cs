using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Umbraco.Composing;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Security;

namespace Umbraco.Web.Common.Security
{
    // TODO: need to implement this

    public class WebSecurity : IWebSecurity
    {
        private readonly IUserService _userService;

        public WebSecurity(IUserService userService)
        {
            _userService = userService;
        }

        private IUser _currentUser;

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <value>The current user.</value>
        public IUser CurrentUser
        {
            get
            {
                //only load it once per instance! (but make sure groups are loaded)
                if (_currentUser == null)
                {
                    var id = GetUserId();
                    _currentUser = id ? _userService.GetUserById(id.Result) : null;
                }

                return _currentUser;
            }
        }

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
