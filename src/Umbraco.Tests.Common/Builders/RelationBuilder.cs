using System;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class RelationBuilder
        : BuilderBase<Relation>,
            IWithIdBuilder,
            IWithKeyBuilder,
            IWithCreateDateBuilder,
            IWithUpdateDateBuilder
    {
        private RelationTypeBuilder _relationTypeBuilder;

        private int? _id;
        private int? _parentId;
        private int? _childId;
        private Guid? _key;
        private DateTime? _createDate;
        private DateTime? _updateDate;
        private string _comment;

        public RelationBuilder WithComment(string comment)
        {
            _comment = comment;
            return this;
        }

        public RelationBuilder BetweenIds(int parentId, int childId)
        {
            _parentId = parentId;
            _childId = childId;
            return this;
        }

        public RelationTypeBuilder AddRelationType()
        {
            var builder = new RelationTypeBuilder(this);
            _relationTypeBuilder = builder;
            return builder;
        }

        public override Relation Build()
        {
            var id = _id ?? 0;
            var parentId = _parentId ?? 0;
            var childId = _childId ?? 0;
            var key = _key ?? Guid.NewGuid();
            var createDate = _createDate ?? DateTime.Now;
            var updateDate = _updateDate ?? DateTime.Now;
            var comment = _comment ?? string.Empty;

            if (_relationTypeBuilder == null)
            {
                throw new InvalidOperationException("Cannot construct a Relation without a RelationType.  Use AddRelationType().");
            }

            var relationType = _relationTypeBuilder.Build();

            Reset();
            return new Relation(parentId, childId, relationType)
                {
                    Comment = comment,
                    CreateDate = createDate,
                    Id = id,
                    Key = key,
                    UpdateDate = updateDate
                };
        }

        protected override void Reset()
        {
            _id = null;
            _parentId = null;
            _childId = null;
            _key = null;
            _createDate = null;
            _updateDate = null;
            _comment = null;
        }

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
        }

        Guid? IWithKeyBuilder.Key
        {
            get => _key;
            set => _key = value;
        }

        DateTime? IWithCreateDateBuilder.CreateDate
        {
            get => _createDate;
            set => _createDate = value;
        }

        DateTime? IWithUpdateDateBuilder.UpdateDate
        {
            get => _updateDate;
            set => _updateDate = value;
        }
    }
}
