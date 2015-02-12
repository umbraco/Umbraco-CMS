using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.Security.Providers
{
    /// <summary>
    /// Custom Membership Provider for Umbraco Members (User authentication for Frontend applications NOT umbraco CMS)  
    /// </summary>
    public class MembersMembershipProvider : UmbracoMembershipProvider<IMembershipMemberService, IMember>, IUmbracoMemberTypeMembershipProvider
    {
        public MembersMembershipProvider()
            : this(ApplicationContext.Current.Services.MemberService)
        {
        }

        public MembersMembershipProvider(IMembershipMemberService<IMember> memberService)
            : base(memberService)
        {
            LockPropertyTypeAlias = Constants.Conventions.Member.IsLockedOut;
            LastLockedOutPropertyTypeAlias = Constants.Conventions.Member.LastLockoutDate;
            FailedPasswordAttemptsPropertyTypeAlias = Constants.Conventions.Member.FailedPasswordAttempts;
            ApprovedPropertyTypeAlias = Constants.Conventions.Member.IsApproved;
            CommentPropertyTypeAlias = Constants.Conventions.Member.Comments;
            LastLoginPropertyTypeAlias = Constants.Conventions.Member.LastLoginDate;
            LastPasswordChangedPropertyTypeAlias = Constants.Conventions.Member.LastPasswordChangeDate;
            PasswordRetrievalQuestionPropertyTypeAlias = Constants.Conventions.Member.PasswordQuestion;
            PasswordRetrievalAnswerPropertyTypeAlias = Constants.Conventions.Member.PasswordAnswer;
        }

        private string _defaultMemberTypeAlias = "Member";
        private volatile bool _hasDefaultMember = false;
        private static readonly object Locker = new object();
        private bool _providerKeyAsGuid = false;

        public override string ProviderName
        {
            get { return "MembersMembershipProvider"; }
        }
        
        protected override MembershipUser ConvertToMembershipUser(IMember entity)
        {
            return entity.AsConcreteMembershipUser(Name, _providerKeyAsGuid);
        }

        public string LockPropertyTypeAlias { get; private set; }
        public string LastLockedOutPropertyTypeAlias { get; private set; }
        public string FailedPasswordAttemptsPropertyTypeAlias { get; private set; }
        public string ApprovedPropertyTypeAlias { get; private set; }
        public string CommentPropertyTypeAlias { get; private set; }
        public string LastLoginPropertyTypeAlias { get; private set; }
        public string LastPasswordChangedPropertyTypeAlias { get; private set; }
        public string PasswordRetrievalQuestionPropertyTypeAlias { get; private set; }
        public string PasswordRetrievalAnswerPropertyTypeAlias { get; private set; }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            // test for membertype (if not specified, choose the first member type available)
            if (config["defaultMemberTypeAlias"] != null)
            {
                _defaultMemberTypeAlias = config["defaultMemberTypeAlias"];
                if (_defaultMemberTypeAlias.IsNullOrWhiteSpace())
                {
                    throw new ProviderException("No default user type alias is specified in the web.config string. Please add a 'defaultUserTypeAlias' to the add element in the provider declaration in web.config");
                }
                _hasDefaultMember = true;
            }

            //devs can configure the provider user key to be a guid if they want, by default it is int
            if (config["providerKeyType"] != null)
            {
                if (config["providerKeyType"] == "guid")
                {
                    _providerKeyAsGuid = true;
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
