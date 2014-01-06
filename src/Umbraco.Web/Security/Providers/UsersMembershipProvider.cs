using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Web.Security.Providers
{
    /// <summary>
    /// Custom Membership Provider for Umbraco Users (User authentication for Umbraco Backend CMS)  
    /// </summary>
    internal class UsersMembershipProvider : UmbracoServiceMembershipProvider<IMembershipUserService, IUser>
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

        protected override MembershipUser ConvertToMembershipUser(IUser entity)
        {
            return entity.AsConcreteMembershipUser(Name);
        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            //TODO: need to determine the default member type!
        }        

        public override string DefaultMemberTypeAlias
        {
            get { return _defaultMemberTypeAlias; }
        }
    }
}