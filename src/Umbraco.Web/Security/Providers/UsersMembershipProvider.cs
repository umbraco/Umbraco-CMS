using System;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using Umbraco.Core;
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

        public override string ProviderName 
        {
            get { return "UsersMembershipProvider"; }
        }

        /// <summary>
        /// For backwards compatibility, this provider supports this option
        /// </summary>
        public override bool AllowManuallyChangingPassword
        {
            get { return true; }
        }

        protected override MembershipUser ConvertToMembershipUser(IUser entity)
        {
            //the provider user key is always the int id
            return entity.AsConcreteMembershipUser(Name);            
        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            // test for membertype (if not specified, choose the first member type available)
            if (config["defaultUserTypeAlias"] != null)
            {
                _defaultMemberTypeAlias = config["defaultUserTypeAlias"];
            }
            else
            {
                var defaultFromService = MemberService.GetDefaultMemberType();
                if (defaultFromService.IsNullOrWhiteSpace())
                {
                    throw new ProviderException("No default user type alias is specified in the web.config string. Please add a 'defaultUserTypeAlias' to the add element in the provider declaration in web.config");
                }
            }    
        }        

        public override string DefaultMemberTypeAlias
        {
            get { return _defaultMemberTypeAlias; }
        }
    }
}