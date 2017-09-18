using System;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.Security.Providers
{
    /// <summary>
    /// Custom Membership Provider for Umbraco Users (User authentication for Umbraco Backend CMS)  
    /// </summary>
    public class UsersMembershipProvider : UmbracoMembershipProvider<IMembershipUserService, IUser>, IUsersMembershipProvider
    {

        public UsersMembershipProvider()
            : this(ApplicationContext.Current.Services.UserService)
        {
        }

        public UsersMembershipProvider(IMembershipMemberService<IUser> memberService)
            : base(memberService)
        {
        }

        private string _defaultMemberTypeAlias = "writer";
        private volatile bool _hasDefaultMember = false;
        private static readonly object Locker = new object();

        public override string ProviderName
        {
            get { return UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider; }
        }

        protected override MembershipUser ConvertToMembershipUser(IUser entity)
        {
            //the provider user key is always the int id
            return entity.AsConcreteMembershipUser(Name, true);
        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            // test for membertype (if not specified, choose the first member type available)
            if (config["defaultUserTypeAlias"] != null)
            {
                _defaultMemberTypeAlias = config["defaultUserTypeAlias"];
                if (_defaultMemberTypeAlias.IsNullOrWhiteSpace())
                {
                    throw new ProviderException("No default user type alias is specified in the web.config string. Please add a 'defaultUserTypeAlias' to the add element in the provider declaration in web.config");
                }
                _hasDefaultMember = true;
            }
        }

        public override string DefaultMemberTypeAlias
        {
            get
            {
                if (_hasDefaultMember == false)
                {
                    lock (Locker)
                    {
                        if (_hasDefaultMember == false)
                        {
                            _defaultMemberTypeAlias = MemberService.GetDefaultMemberType();
                            if (_defaultMemberTypeAlias.IsNullOrWhiteSpace())
                            {
                                throw new ProviderException("No default user type alias is specified in the web.config string. Please add a 'defaultUserTypeAlias' to the add element in the provider declaration in web.config");
                            }
                            _hasDefaultMember = true;
                        }
                    }
                }
                return _defaultMemberTypeAlias;
            }
        }

        /// <summary>
        /// Overridden in order to call the BackOfficeUserManager.UnlockUser method in order to raise the user audit events
        /// </summary>
        /// <param name="username"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        internal override bool PerformUnlockUser(string username, out IUser member)
        {
            var result = base.PerformUnlockUser(username, out member);
            if (result)
            {
                var userManager = GetBackofficeUserManager();
                if (userManager != null)
                {
                    userManager.RaiseAccountUnlockedEvent(member.Id);
                }   
            }
            return result;
        }

        /// <summary>
        /// Override in order to raise appropriate events via the <see cref="BackOfficeUserManager"/>
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal override ValidateUserResult PerformValidateUser(string username, string password)
        {
            var result = base.PerformValidateUser(username, password);

            var userManager = GetBackofficeUserManager();

            if (userManager == null) return result;

            if (result.Authenticated == false)
            {
                var count = result.Member.FailedPasswordAttempts;
                if (count >= MaxInvalidPasswordAttempts)
                {
                    userManager.RaiseAccountLockedEvent(result.Member.Id);
                }
            }
            else
            {
                if (result.Member.FailedPasswordAttempts > 0)
                {
                    //we have successfully logged in, if the failed password attempts was modified it means it was reset
                    if (result.Member.WasPropertyDirty("FailedPasswordAttempts"))
                    {                        
                        userManager.RaiseResetAccessFailedCountEvent(result.Member.Id);
                    }
                }
            }

            return result;
        }

        internal BackOfficeUserManager<BackOfficeIdentityUser> GetBackofficeUserManager()
        {
            return HttpContext.Current == null
                ? null
                : HttpContext.Current.GetOwinContext().GetBackOfficeUserManager();
        }
    }
}