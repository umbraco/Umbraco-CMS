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
    public class MembersMembershipProvider : UmbracoServiceMembershipProvider<IMembershipMemberService, IMember>, IUmbracoContentTypeMembershipProvider
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

        public override string ProviderName
        {
            get { return "MembersMembershipProvider"; }
        }

        /// <summary>
        /// For backwards compatibility, this provider supports this option
        /// </summary>
        public override bool AllowManuallyChangingPassword
        {
            get { return true; }
        }

        protected override MembershipUser ConvertToMembershipUser(IMember entity)
        {
            return entity.AsConcreteMembershipUser(Name);
        }

        public string LockPropertyTypeAlias { get; protected set; }
        public string LastLockedOutPropertyTypeAlias { get; protected set; }
        public string FailedPasswordAttemptsPropertyTypeAlias { get; protected set; }
        public string ApprovedPropertyTypeAlias { get; protected set; }
        public string CommentPropertyTypeAlias { get; protected set; }
        public string LastLoginPropertyTypeAlias { get; protected set; }
        public string LastPasswordChangedPropertyTypeAlias { get; protected set; }
        public string PasswordRetrievalQuestionPropertyTypeAlias { get; protected set; }
        public string PasswordRetrievalAnswerPropertyTypeAlias { get; protected set; }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            // test for membertype (if not specified, choose the first member type available)
            if (config["defaultMemberTypeAlias"] != null)
            {
                _defaultMemberTypeAlias = config["defaultMemberTypeAlias"];
            }
            else
            {
                _defaultMemberTypeAlias = MemberService.GetDefaultMemberType();
                if (_defaultMemberTypeAlias.IsNullOrWhiteSpace())
                {
                    throw new ProviderException("No default MemberType alias is specified in the web.config string. Please add a 'defaultMemberTypeAlias' to the add element in the provider declaration in web.config");
                }
            }

            // test for approve status
            if (config["umbracoApprovePropertyTypeAlias"] != null)
            {
                ApprovedPropertyTypeAlias = config["umbracoApprovePropertyTypeAlias"];
            }
            // test for lock attempts
            if (config["umbracoLockPropertyTypeAlias"] != null)
            {
                LockPropertyTypeAlias = config["umbracoLockPropertyTypeAlias"];
            }
            if (config["umbracoLastLockedPropertyTypeAlias"] != null)
            {
                LastLockedOutPropertyTypeAlias = config["umbracoLastLockedPropertyTypeAlias"];
            }
            if (config["umbracoLastPasswordChangedPropertyTypeAlias"] != null)
            {
                LastPasswordChangedPropertyTypeAlias = config["umbracoLastPasswordChangedPropertyTypeAlias"];
            }
            if (config["umbracoFailedPasswordAttemptsPropertyTypeAlias"] != null)
            {
                FailedPasswordAttemptsPropertyTypeAlias = config["umbracoFailedPasswordAttemptsPropertyTypeAlias"];
            }
            // comment property
            if (config["umbracoCommentPropertyTypeAlias"] != null)
            {
                CommentPropertyTypeAlias = config["umbracoCommentPropertyTypeAlias"];
            }
            // last login date
            if (config["umbracoLastLoginPropertyTypeAlias"] != null)
            {
                LastLoginPropertyTypeAlias = config["umbracoLastLoginPropertyTypeAlias"];
            }
            // password retrieval
            if (config["umbracoPasswordRetrievalQuestionPropertyTypeAlias"] != null)
            {
                PasswordRetrievalQuestionPropertyTypeAlias = config["umbracoPasswordRetrievalQuestionPropertyTypeAlias"];
            }
            if (config["umbracoPasswordRetrievalAnswerPropertyTypeAlias"] != null)
            {
                PasswordRetrievalAnswerPropertyTypeAlias = config["umbracoPasswordRetrievalAnswerPropertyTypeAlias"];
            }
        }

        public override string DefaultMemberTypeAlias
        {
            get { return _defaultMemberTypeAlias; }
        }
    }
}