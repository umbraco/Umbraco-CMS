using System.Collections.Generic;
using System.Linq;
using Moq;
using Umbraco.Core.Models.Membership;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class UserGroupBuilder
        : BuilderBase<IUserGroup>,
            IWithIdBuilder
    {
        private int? _startContentId;
        private int? _startMediaId;
        private string _alias;
        private string _icon;
        private string _name;
        private IEnumerable<string> _permissions = Enumerable.Empty<string>();
        private IEnumerable<string> _sectionCollection = Enumerable.Empty<string>();
        private string _suffix;
        private int? _id;

        /// <summary>
        /// Will suffix the name and alias for testing
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public UserGroupBuilder WithSuffix(string suffix)
        {
            _suffix = suffix;
            return this;
        }

        public IReadOnlyUserGroup BuildReadOnly(IUserGroup userGroup)
        {
            return Mock.Of<IReadOnlyUserGroup>(x =>
                x.Permissions == userGroup.Permissions &&
                x.Alias == userGroup.Alias &&
                x.Icon == userGroup.Icon &&
                x.Name == userGroup.Name &&
                x.StartContentId == userGroup.StartContentId &&
                x.StartMediaId == userGroup.StartMediaId &&
                x.AllowedSections == userGroup.AllowedSections &&
                x.Id == userGroup.Id);
        }

        public override IUserGroup Build()
        {
            return Mock.Of<IUserGroup>(x =>
                x.StartContentId == _startContentId &&
                x.StartMediaId == _startMediaId &&
                x.Name == (_name ?? ("TestUserGroup" + _suffix)) &&
                x.Alias == (_alias ?? ("testUserGroup" + _suffix)) &&
                x.Icon == _icon &&
                x.Permissions == _permissions &&
                x.AllowedSections == _sectionCollection);
        }

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
        }
    }
}
