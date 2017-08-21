using System;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Configuration;
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

        private bool _allowManuallyChangingPassword = false;
        private bool _enablePasswordReset = false;

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to reset their passwords.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider supports password reset; otherwise, false. The default is FALSE for users.</returns>
        public override bool EnablePasswordReset
        {
            get { return _enablePasswordReset; }
        }

        /// <summary>
        /// For backwards compatibility, this provider supports this option by default it is FALSE for users
        /// </summary>
        public override bool AllowManuallyChangingPassword
        {
            get { return _allowManuallyChangingPassword; }
        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            if (config == null) { throw new ArgumentNullException("config"); }
            
            _allowManuallyChangingPassword = config.GetValue("allowManuallyChangingPassword", false);
            _enablePasswordReset = config.GetValue("enablePasswordReset", false);

            // test for membertype (if not specified, choose the first member type available)
            // We'll support both names for legacy reasons: defaultUserTypeAlias & defaultUserGroupAlias

            if (config["defaultUserTypeAlias"] != null)
            {
                if (config["defaultUserTypeAlias"].IsNullOrWhiteSpace() == false)
                {
                    _defaultMemberTypeAlias = config["defaultUserTypeAlias"];
                    _hasDefaultMember = true;
                }
            }
            if (_hasDefaultMember == false && config["defaultUserGroupAlias"] != null)
            {                
                if (config["defaultUserGroupAlias"].IsNullOrWhiteSpace() == false)
                {
                    _defaultMemberTypeAlias = config["defaultUserGroupAlias"];
                    _hasDefaultMember = true;
                }
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
                                throw new ProviderException("No default user group alias is specified in the web.config string. Please add a 'defaultUserTypeAlias' to the add element in the provider declaration in web.config");
                            }
                            _hasDefaultMember = true;
                        }
                    }
                }
                return _defaultMemberTypeAlias;
            }
        }
    }
}