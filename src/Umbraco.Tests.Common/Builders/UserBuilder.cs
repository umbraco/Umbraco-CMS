using System;
using System.Linq;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Membership;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class UserBuilder
        : BuilderBase<User>,
            IWithIdBuilder
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

        public UserBuilder WithDefaultUILanguage(string defaultLang)
        {
            _defaultLang = defaultLang;
            return this;
        }

        public UserBuilder WithLanguage(string language)
        {
            _language = language;
            return this;
        }

        public UserBuilder WithApproved(bool approved)
        {
            _approved = approved;
            return this;
        }

        public UserBuilder WithRawPassword(string rawPassword)
        {
            _rawPassword = rawPassword;
            return this;
        }

        public UserBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public UserBuilder WithUsername(string username)
        {
            _username = username;
            return this;
        }

        public UserBuilder WithLockedOut(bool isLockedOut)
        {
            _isLockedOut = isLockedOut;
            return this;
        }

        public UserBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Will suffix the name, email and username for testing
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public UserBuilder WithSuffix(string suffix)
        {
            _suffix = suffix;
            return this;
        }

        public override User Build()
        {
            var globalSettings = Mock.Of<IGlobalSettings>(x => x.DefaultUILanguage == (_defaultLang ?? "en-US"));
            return new User(globalSettings,
                _name ?? "TestUser" + _suffix,
                _email ?? "test" + _suffix + "@test.com",
                _username ?? "TestUser" + _suffix,
                _rawPassword ?? "abcdefghijklmnopqrstuvwxyz")
            {
                Language = _language ?? _defaultLang ?? "en-US",
                IsLockedOut = _isLockedOut ?? false,
                IsApproved = _approved ?? true
            };
        }

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
        }
    }
}
