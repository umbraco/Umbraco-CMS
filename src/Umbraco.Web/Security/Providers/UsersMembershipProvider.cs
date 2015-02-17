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
    }
}