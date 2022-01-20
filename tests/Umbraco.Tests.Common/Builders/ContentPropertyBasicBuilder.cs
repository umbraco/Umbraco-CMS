// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders
{
    public class ContentPropertyBasicBuilder<TParent> : ChildBuilderBase<TParent, ContentPropertyBasic>,
        IWithIdBuilder, IWithAliasBuilder
    {
        private int? _id;
        private string _alias;
        private object _value;

        public ContentPropertyBasicBuilder(TParent parentBuilder)
            : base(parentBuilder)
        {
        }

        public override ContentPropertyBasic Build()
        {
            var alias = _alias ?? null;
            var id = _id ?? 0;
            var value = _value ?? null;

            return new ContentPropertyBasic()
            {
                Alias = alias,
                Id = id,
                Value = value
            };
        }

        public ContentPropertyBasicBuilder<TParent> WithValue(object value)
        {
            _value = value;
            return this;
        }

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
        }

        string IWithAliasBuilder.Alias
        {
            get => _alias;
            set => _alias = value;
        }
    }
}
