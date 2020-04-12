using Umbraco.Core.Models.Entities;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class EntitySlimBuilder
        : BuilderBase<EntitySlim>,
            IWithIdBuilder,
            IWithParentIdBuilder
    {
        private int? _id;
        private int? _parentId;

        public override EntitySlim Build()
        {
            var id = _id ?? 1;
            var parentId = _parentId ?? -1;

            Reset();
            return new EntitySlim
            {
                Id = id,
                ParentId = parentId,
            };
        }

        protected override void Reset()
        {
            _id = null;
            _parentId = null;
        }

        public EntitySlimBuilder WithNoParentId()
        {
            _parentId = 0;
            return this;
        }

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
        }

        int? IWithParentIdBuilder.ParentId
        {
            get => _parentId;
            set => _parentId = value;
        }
    }
}
