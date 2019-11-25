using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Composing;
using System;

namespace Umbraco.Web.Security.Providers
{
    /// <summary>
    /// Custom Membership Provider for Umbraco Members (User authentication for Frontend applications NOT umbraco CMS)
    /// </summary>
    public class MembersMembershipProvider : UmbracoMembershipProvider<IMembershipMemberService, IMember>, IUmbracoMemberTypeMembershipProvider
    {
        public MembersMembershipProvider()
            : this(Current.Services.MemberService, Current.Services.MemberTypeService, Current.UmbracoVersion)
        { }

        public MembersMembershipProvider(IMembershipMemberService<IMember> memberService, IMemberTypeService memberTypeService, IUmbracoVersion umbracoVersion)
            : base(memberService, umbracoVersion)
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
            _memberTypeService = memberTypeService;
        }

        private readonly IMemberTypeService _memberTypeService;
        private string _defaultMemberTypeAlias = "Member";
        private volatile bool _hasDefaultMember;
        private static readonly object Locker = new object();
        private bool _providerKeyAsGuid;

        public override string ProviderName => "MembersMembershipProvider";

        protected override MembershipUser ConvertToMembershipUser(IMember entity)
        {
            return entity.AsConcreteMembershipUser(Name, _providerKeyAsGuid);
        }

        public string LockPropertyTypeAlias { get; }
        public string LastLockedOutPropertyTypeAlias { get; }
        public string FailedPasswordAttemptsPropertyTypeAlias { get; }
        public string ApprovedPropertyTypeAlias { get; }
        public string CommentPropertyTypeAlias { get; }
        public string LastLoginPropertyTypeAlias { get; }
        public string LastPasswordChangedPropertyTypeAlias { get; }
        public string PasswordRetrievalQuestionPropertyTypeAlias { get; }
        public string PasswordRetrievalAnswerPropertyTypeAlias { get; }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            // test for membertype (if not specified, choose the first member type available)
            if (config["defaultMemberTypeAlias"] != null)
            {
                _defaultMemberTypeAlias = config["defaultMemberTypeAlias"];
                if (_defaultMemberTypeAlias.IsNullOrWhiteSpace())
                {
                    throw new ProviderException("No default member type alias is specified in the web.config string. Please add a 'defaultUserTypeAlias' to the add element in the provider declaration in web.config");
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

            PasswordConfiguration = new MembershipProviderPasswordConfiguration(
                    MinRequiredPasswordLength,
                    MinRequiredNonAlphanumericCharacters > 0,
                    false, false, false, UseLegacyEncoding,
                    CustomHashAlgorithmType ?? Membership.HashAlgorithmType,
                    MaxInvalidPasswordAttempts);

            _passwordSecurity = new PasswordSecurity(PasswordConfiguration);

        }

        protected override Attempt<string> GetRawPassword(string username)
        {
            var found = MemberService.GetByUsername(username);
            if (found == null) return Attempt<string>.Fail();
            return Attempt.Succeed(found.RawPasswordValue);
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
                            _defaultMemberTypeAlias = _memberTypeService.GetDefault();
                            if (_defaultMemberTypeAlias.IsNullOrWhiteSpace())
                            {
                                throw new ProviderException("No default member type alias is specified in the web.config string. Please add a 'defaultUserTypeAlias' to the add element in the provider declaration in web.config");
                            }
                            _hasDefaultMember = true;
                        }
                    }
                }
                return _defaultMemberTypeAlias;
            }
        }

        private PasswordSecurity _passwordSecurity;

        public override PasswordSecurity PasswordSecurity => _passwordSecurity;
        public IPasswordConfiguration PasswordConfiguration { get; private set; }

        private class MembershipProviderPasswordConfiguration : IPasswordConfiguration
        {
            public MembershipProviderPasswordConfiguration(int requiredLength, bool requireNonLetterOrDigit, bool requireDigit, bool requireLowercase, bool requireUppercase, bool useLegacyEncoding, string hashAlgorithmType, int maxFailedAccessAttemptsBeforeLockout)
            {
                RequiredLength = requiredLength;
                RequireNonLetterOrDigit = requireNonLetterOrDigit;
                RequireDigit = requireDigit;
                RequireLowercase = requireLowercase;
                RequireUppercase = requireUppercase;
                UseLegacyEncoding = useLegacyEncoding;
                HashAlgorithmType = hashAlgorithmType ?? throw new ArgumentNullException(nameof(hashAlgorithmType));
                MaxFailedAccessAttemptsBeforeLockout = maxFailedAccessAttemptsBeforeLockout;
            }

            public int RequiredLength { get; }

            public bool RequireNonLetterOrDigit { get; }

            public bool RequireDigit { get; }

            public bool RequireLowercase { get; }

            public bool RequireUppercase { get; }

            public bool UseLegacyEncoding { get; }

            public string HashAlgorithmType { get; }

            public int MaxFailedAccessAttemptsBeforeLockout { get; }
        }
    }
}
