// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

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
    private DateTime? _createDate;
    private int? _creatorId;

    private int? _id;
    private Guid? _key;
    private string _name;
    private DateTime? _updateDate;

    DateTime? IWithCreateDateBuilder.CreateDate
    {
        get => _createDate;
        set => _createDate = value;
    }

    int? IWithCreatorIdBuilder.CreatorId
    {
        get => _creatorId;
        set => _creatorId = value;
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

    string IWithNameBuilder.Name
    {
        get => _name;
        set => _name = value;
    }

    DateTime? IWithUpdateDateBuilder.UpdateDate
    {
        get => _updateDate;
        set => _updateDate = value;
    }

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
            CreatorId = creatorId
        };

        if (_additionalDataBuilder != null)
        {
            var additionalData = _additionalDataBuilder.Build();
            foreach (var kvp in additionalData)
            {
                memberGroup.AdditionalData.Add(kvp.Key, kvp.Value);
            }
        }

        return memberGroup;
    }
}
