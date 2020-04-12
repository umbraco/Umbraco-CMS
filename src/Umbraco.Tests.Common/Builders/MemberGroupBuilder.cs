using System;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class MemberGroupBuilder
        : BuilderBase<MemberGroup>,
            IWithIdBuilder,
            IWithKeyBuilder,
            IWithCreatorIdBuilder,
            IWithCreateDateBuilder,
            IWithUpdateDateBuilder,
            IWithNameBuilder
    {
        private GenericDictionaryBuilder<MemberGroupBuilder, string, object> _additionalDataBuilder;

        private int? _id;
        private Guid? _key;
        private DateTime? _createDate;
        private DateTime? _updateDate;
        private string _name;
        private int? _creatorId;

        public GenericDictionaryBuilder<MemberGroupBuilder, string, object> AddAdditionalData()
        {
            var builder = new GenericDictionaryBuilder<MemberGroupBuilder, string, object>(this);
            _additionalDataBuilder = builder;
            return builder;
        }

        public override MemberGroup Build()
        {
            var id = _id ?? 1;
            var key = _key ?? Guid.NewGuid();
            var createDate = _createDate ?? DateTime.Now;
            var updateDate = _updateDate ?? DateTime.Now;
            var name = _name ?? Guid.NewGuid().ToString();
            var creatorId = _creatorId ?? 1;

            var memberGroup = new MemberGroup
            {
                Id = id,
                Key = key,
                CreateDate = createDate,
                UpdateDate = updateDate,
                Name = name,
                CreatorId = creatorId,
            };

            if (_additionalDataBuilder != null)
            {
                var additionalData = _additionalDataBuilder.Build();
                foreach (var kvp in additionalData)
                {
                    memberGroup.AdditionalData.Add(kvp.Key, kvp.Value);
                }
            }

            Reset();
            return memberGroup;
        }

        protected override void Reset()
        {
            _id = null;
            _key = null;
            _createDate = null;
            _updateDate = null;
            _name = null;
            _creatorId = null;
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

        int? IWithCreatorIdBuilder.CreatorId
        {
            get => _creatorId;
            set => _creatorId = value;
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

        string IWithNameBuilder.Name
        {
            get => _name;
            set => _name = value;
        }
    }
}
