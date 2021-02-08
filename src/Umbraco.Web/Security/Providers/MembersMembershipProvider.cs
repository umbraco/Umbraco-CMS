using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using System;
using Umbraco.Net;

namespace Umbraco.Web.Security.Providers
{
    //TODO: Delete: should not be used
    [Obsolete("We are now using ASP.NET Core Identity instead of membership providers")]
    /// <summary>
    /// Custom Membership Provider for Umbraco Members (User authentication for Frontend applications NOT umbraco CMS)
    /// </summary>
    public class MembersMembershipProvider : UmbracoMembershipProvider<IMembershipMemberService, IMember>
    {
        public MembersMembershipProvider()
            : this(Current.Services.MemberService, Current.Services.MemberTypeService, Current.UmbracoVersion, Current.HostingEnvironment, Current.IpResolver)
        { }

        public MembersMembershipProvider(IMembershipMemberService<IMember> memberService, IMemberTypeService memberTypeService, IUmbracoVersion umbracoVersion, IHostingEnvironment hostingEnvironment, IIpResolver ipResolver)
            : base(memberService, umbracoVersion, hostingEnvironment, ipResolver)
        {
            LockPropertyTypeAlias = Constants.Conventions.Member.IsLockedOut;
            LastLockedOutPropertyTypeAlias = Constants.Conventions.Member.LastLockoutDate;
            FailedPasswordAttemptsPropertyTypeAlias = Constants.Conventions.Member.FailedPasswordAttempts;
            ApprovedPropertyTypeAlias = Constants.Conventions.Member.IsApproved;
            CommentPropertyTypeAlias = Constants.Conventions.Member.Comments;
            LastLoginPropertyTypeAlias = Constants.Conventions.Member.LastLoginDate;
            LastPasswordChangedPropertyTypeAlias = Constants.Conventions.Member.LastPasswordChangeDate;
            _memberTypeService = memberTypeService;
        }

        private readonly IMemberTypeService _memberTypeService;
        private string _defaultMemberTypeAlias = "Member";
        private volatile bool _hasDefaultMember;
        private static readonly object Locker = new object();

        public override string ProviderName => "MembersMembershipProvider";

        protected override MembershipUser ConvertToMembershipUser(IMember entity)
        {
            return entity.AsConcreteMembershipUser(Name);
        }

        public string LockPropertyTypeAlias { get; }
        public string LastLockedOutPropertyTypeAlias { get; }
        public string FailedPasswordAttemptsPropertyTypeAlias { get; }
        public string ApprovedPropertyTypeAlias { get; }
        public string CommentPropertyTypeAlias { get; }
        public string LastLoginPropertyTypeAlias { get; }
        public string LastPasswordChangedPropertyTypeAlias { get; }

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

            // these need to be lazy else we get a stack overflow since we cannot access Membership.HashAlgorithmType without initializing the providers

            _passwordConfig = new Lazy<IPasswordConfiguration>(() => new MembershipProviderPasswordConfiguration(
                    MinRequiredPasswordLength,
                    MinRequiredNonAlphanumericCharacters > 0,
                    false, false, false, UseLegacyEncoding,
                    CustomHashAlgorithmType.IsNullOrWhiteSpace() ? Membership.HashAlgorithmType : CustomHashAlgorithmType,
                    MaxInvalidPasswordAttempts));

            _passwordSecurity = new Lazy<LegacyPasswordSecurity>(() => new LegacyPasswordSecurity());

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

        private Lazy<LegacyPasswordSecurity> _passwordSecurity;
        private Lazy<IPasswordConfiguration> _passwordConfig;

        public override LegacyPasswordSecurity PasswordSecurity => _passwordSecurity.Value;
        public IPasswordConfiguration PasswordConfiguration => _passwordConfig.Value;

        [Obsolete("We are now using ASP.NET Core Identity instead of membership providers")]
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
