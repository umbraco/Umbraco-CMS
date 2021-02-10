// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders
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
            Guid key = _key ?? Guid.NewGuid();
            DateTime createDate = _createDate ?? DateTime.Now;
            DateTime updateDate = _updateDate ?? DateTime.Now;
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
                IDictionary<string, object> additionalData = _additionalDataBuilder.Build();
                foreach (KeyValuePair<string, object> kvp in additionalData)
                {
                    memberGroup.AdditionalData.Add(kvp.Key, kvp.Value);
                }
            }

            return memberGroup;
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
