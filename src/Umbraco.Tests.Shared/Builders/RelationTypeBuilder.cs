using System;
using Umbraco.Core.Models;
using Umbraco.Tests.Shared.Builders.Markers;

namespace Umbraco.Tests.Shared.Builders
{

    public class RelationTypeBuilder : RelationTypeBuilder<object>
    {
        public RelationTypeBuilder() : base(null)
        {
        }
    }

    public class RelationTypeBuilder<TParent> : ChildBuilderBase<TParent, IRelationType>, IWithIdBuilder, IWithAliasBuilder, IWithNameBuilder
    {
        private int? _id;
        private string _alias;
        private string _name;
        private readonly Guid? _parentObjectType = null;
        private readonly Guid? _childObjectType = null;

        public RelationTypeBuilder(TParent parentBuilder) : base(parentBuilder)
        {
        }
        

        public override IRelationType Build()
        {
            var alias = _alias ?? Guid.NewGuid().ToString();
            var name = _name ?? Guid.NewGuid().ToString();
            var parentObjectType = _parentObjectType ?? Guid.NewGuid();
            var childObjectType = _childObjectType ?? Guid.NewGuid();
            var id = _id ?? 1;

            return new RelationType(name, alias, false, parentObjectType,
                childObjectType)
            {
                Id = id
            };
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

        string IWithNameBuilder.Name
        {
            get => _name;
            set => _name = value;
        }
    }
}
