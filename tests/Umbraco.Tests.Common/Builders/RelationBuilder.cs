// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders
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
        private IRelationType _relationType;

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

        public RelationBuilder WithRelationType(IRelationType relationType)
        {
            _relationType = relationType;
            _relationTypeBuilder = null;
            return this;
        }

        public RelationTypeBuilder AddRelationType()
        {
            _relationType = null;
            var builder = new RelationTypeBuilder(this);
            _relationTypeBuilder = builder;
            return builder;
        }

        public override Relation Build()
        {
            var id = _id ?? 0;
            var parentId = _parentId ?? 0;
            var childId = _childId ?? 0;
            Guid key = _key ?? Guid.NewGuid();
            DateTime createDate = _createDate ?? DateTime.Now;
            DateTime updateDate = _updateDate ?? DateTime.Now;
            var comment = _comment ?? string.Empty;

            if (_relationTypeBuilder == null && _relationType == null)
            {
                throw new InvalidOperationException("Cannot construct a Relation without a RelationType.  Use AddRelationType() or WithRelationType().");
            }

            IRelationType relationType = _relationType ?? _relationTypeBuilder.Build();

            return new Relation(parentId, childId, relationType)
                {
                    Comment = comment,
                    CreateDate = createDate,
                    Id = id,
                    Key = key,
                    UpdateDate = updateDate
                };
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
