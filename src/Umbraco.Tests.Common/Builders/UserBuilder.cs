using Umbraco.Core.Models.Membership;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{

    public class UserBuilder : UserBuilder<object>
    {
        public UserBuilder() : base(null)
        {
        }
    }

    public class UserBuilder<TParent>
        : ChildBuilderBase<TParent, User>,
            IWithIdBuilder,
            IWithNameBuilder,
            IWithApprovedBuilder
    {
        private int? _id;
        private string _language;
        private bool? _approved;
        private string _name;
        private string _rawPassword;
        private bool? _isLockedOut;
        private string _email;
        private string _username;
        private string _defaultLang;
        private string _suffix = string.Empty;
        private GlobalSettingsBuilder<UserBuilder<TParent>> _globalSettingsBuilder;


        public UserBuilder(TParent parentBuilder) : base(parentBuilder)
        {
            _globalSettingsBuilder = new GlobalSettingsBuilder<UserBuilder<TParent>>(this);
        }

        public GlobalSettingsBuilder<UserBuilder<TParent>> AddGlobalSettings() => _globalSettingsBuilder;
        public UserBuilder<TParent> WithDefaultUILanguage(string defaultLang)
        {
            _defaultLang = defaultLang;
            return this;
        }

        public UserBuilder<TParent> WithLanguage(string language)
        {
            _language = language;
            return this;
        }

        public UserBuilder<TParent> WithRawPassword(string rawPassword)
        {
            _rawPassword = rawPassword;
            return this;
        }

        public UserBuilder<TParent> WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public UserBuilder<TParent> WithUsername(string username)
        {
            _username = username;
            return this;
        }

        public UserBuilder<TParent> WithLockedOut(bool isLockedOut)
        {
            _isLockedOut = isLockedOut;
            return this;
        }

        /// <summary>
        /// Will suffix the name, email and username for testing
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public UserBuilder<TParent> WithSuffix(string suffix)
        {
            _suffix = suffix;
            return this;
        }

        public override User Build()
        {
            var globalSettings = _globalSettingsBuilder.Build();
            var name = _name ?? "TestUser" + _suffix;
            var email = _email ?? "test" + _suffix + "@test.com";
            var username = _username ?? "TestUser" + _suffix;
            var rawPassword = _rawPassword ?? "abcdefghijklmnopqrstuvwxyz";
            var language = _language ?? globalSettings.DefaultUILanguage;
            var isLockedOut = _isLockedOut ?? false;
            var approved = _approved ?? true;

            return new User(
                globalSettings,
                name,
                email,
                username,
                rawPassword)
            {
                Language = language,
                IsLockedOut = isLockedOut,
                IsApproved = approved
            };
        }

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
        }

        string IWithNameBuilder.Name
        {
            get => _name;
            set => _name = value;
        }

        bool? IWithApprovedBuilder.Approved
        {
            get => _approved;
            set => _approved = value;
        }
    }
}
