// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders
{
    public class UmbracoIdentityRoleBuilder : BuilderBase<UmbracoIdentityRole>,
            IWithIdBuilder<string>,
            IWithNameBuilder
    {
        private string _id;
        private string _name;

        public UmbracoIdentityRoleBuilder WithTestName(string id)
        {
            _name = "testname";
            _id = id;
            return this;
        }

        string IWithNameBuilder.Name
        {
            get => _name;
            set => _name = value;
        }

        string IWithIdBuilder<string>.Id
        {
            get => _id;
            set => _id = value;
        }

        public override UmbracoIdentityRole Build()
        {
            var id = _id;
            var name = _name;

            return new UmbracoIdentityRole
            {
                Id = id,
                Name = name,
            };
        }
    }
}
