using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Web.Models;

namespace Umbraco.Web.Security
{
    internal class MembershipHelper
    {
        private readonly ApplicationContext _applicationContext;
        private readonly HttpContextBase _httpContext;

        public MembershipHelper(ApplicationContext applicationContext, HttpContextBase httpContext)
        {
            if (applicationContext == null) throw new ArgumentNullException("applicationContext");
            if (httpContext == null) throw new ArgumentNullException("httpContext");
            _applicationContext = applicationContext;
            _httpContext = httpContext;
        }

        public MembershipHelper(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            _httpContext = umbracoContext.HttpContext;
            _applicationContext = umbracoContext.Application;
        }

        public LoginStatusModel GetLoginStatusModel()
        {
            if (_httpContext.User == null || _httpContext.User.Identity.IsAuthenticated == false) 
                return null;

            var member = _applicationContext.Services.MemberService.GetByUsername(
                _httpContext.User.Identity.Name);

            var model = LoginStatusModel.CreateModel();
            model.Name = member.Name;
            model.Username = member.Username;
            model.Email = member.Email;
            model.IsLoggedIn = true;
            return model;
        }

        public MembershipUser UpdateMember(MembershipUser member, MembershipProvider provider,
            string email = null,
            bool? isApproved = null,
            bool? isLocked = null,
            DateTime? lastLoginDate = null,
            DateTime? lastActivityDate = null,
            string comment = null)
        {
            //set the writable properties
            if (email != null)
            {
                member.Email = email;    
            }
            if (isApproved.HasValue)
            {
                member.IsApproved = isApproved.Value;    
            }
            if (lastLoginDate.HasValue)
            {
                member.LastLoginDate = lastLoginDate.Value;
            }
            if (lastActivityDate.HasValue)
            {
                member.LastActivityDate = lastActivityDate.Value;
            }
            if (comment != null)
            {
                member.Comment = comment;
            }

            if (isLocked.HasValue)
            {
                //there is no 'setter' on IsLockedOut but you can ctor a new membership user with it set, so i guess that's what we'll do,
                // this does mean however if it was a typed membership user object that it will no longer be typed
                //membershipUser.IsLockedOut = true;
                member = new MembershipUser(member.ProviderName, member.UserName, 
                    member.ProviderUserKey, member.Email, member.PasswordQuestion, member.Comment, member.IsApproved, 
                    isLocked.Value,  //new value
                    member.CreationDate, member.LastLoginDate, member.LastActivityDate, member.LastPasswordChangedDate, member.LastLockoutDate);
            }

            provider.UpdateUser(member);

            return member;
        }

    }
}
