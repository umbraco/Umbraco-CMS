using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.Security.Providers
{
    /// <summary>
    /// Custom Membership Provider for Umbraco Members (User authentication for Frontend applications NOT umbraco CMS)  
    /// </summary>
    internal class MembersMembershipProvider : UmbracoServiceMembershipProvider<IMembershipMemberService, IMember>
    {
        public MembersMembershipProvider()
            : this(ApplicationContext.Current.Services.MemberService)
        {            
        }

        public MembersMembershipProvider(IMembershipMemberService<IMember> memberService) : base(memberService)
        {
        }

        private string _defaultMemberTypeAlias = "Member";

        public override string ProviderName
        {
            get { return "MembersMembershipProvider"; }
        }

        protected override MembershipUser ConvertToMembershipUser(IMember entity)
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