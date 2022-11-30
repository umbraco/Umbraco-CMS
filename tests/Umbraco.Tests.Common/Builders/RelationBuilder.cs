// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class RelationBuilder
    : BuilderBase<Relation>,
        IWithIdBuilder,
        IWithKeyBuilder,
        IWithCreateDateBuilder,
        IWithUpdateDateBuilder
{
    private int? _childId;
    private string _comment;
    private DateTime? _createDate;

    private int? _id;
    private Guid? _key;
    private int? _parentId;
    private IRelationType _relationType;
    private RelationTypeBuilder _relationTypeBuilder;
    private DateTime? _updateDate;

    DateTime? IWithCreateDateBuilder.CreateDate
    {
        get => _createDate;
        set => _createDate = value;
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

    DateTime? IWithUpdateDateBuilder.UpdateDate
    {
        get => _updateDate;
        set => _updateDate = value;
    }

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
        var key = _key ?? Guid.NewGuid();
        var createDate = _createDate ?? DateTime.Now;
        var updateDate = _updateDate ?? DateTime.Now;
        var comment = _comment ?? string.Empty;

        if (_relationTypeBuilder == null && _relationType == null)
        {
            throw new InvalidOperationException(
                "Cannot construct a Relation without a RelationType.  Use AddRelationType() or WithRelationType().");
        }

        var relationType = _relationType ?? _relationTypeBuilder.Build();

        return new Relation(parentId, childId, relationType)
        {
            Comment = comment,
            CreateDate = createDate,
            Id = id,
            Key = key,
            UpdateDate = updateDate
        };
    }
}
