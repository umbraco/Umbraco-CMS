// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class RelationTypeBuilder
    : ChildBuilderBase<RelationBuilder, IRelationTypeWithIsDependency>,
        IWithIdBuilder,
        IWithAliasBuilder,
        IWithNameBuilder,
        IWithKeyBuilder,
        IWithCreateDateBuilder,
        IWithUpdateDateBuilder,
        IWithDeleteDateBuilder
{
    private string _alias;
    private Guid? _childObjectType;
    private DateTime? _createDate;
    private DateTime? _deleteDate;
    private int? _id;
    private bool? _isBidirectional;
    private bool? _isDependency;
    private Guid? _key;
    private string _name;
    private Guid? _parentObjectType;
    private DateTime? _updateDate;

    public RelationTypeBuilder()
        : base(null)
    {
    }

    public RelationTypeBuilder(RelationBuilder parentBuilder)
        : base(parentBuilder)
    {
    }

    string IWithAliasBuilder.Alias
    {
        get => _alias;
        set => _alias = value;
    }

    DateTime? IWithCreateDateBuilder.CreateDate
    {
        get => _createDate;
        set => _createDate = value;
    }

    DateTime? IWithDeleteDateBuilder.DeleteDate
    {
        get => _deleteDate;
        set => _deleteDate = value;
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

    public RelationTypeBuilder WithIsBidirectional(bool isBidirectional)
    {
        _isBidirectional = isBidirectional;
        return this;
    }

    public RelationTypeBuilder WithIsDependency(bool isDependency)
    {
        _isDependency = isDependency;
        return this;
    }

    public RelationTypeBuilder WithChildObjectType(Guid childObjectType)
    {
        _childObjectType = childObjectType;
        return this;
    }

    public RelationTypeBuilder WithParentObjectType(Guid parentObjectType)
    {
        _parentObjectType = parentObjectType;
        return this;
    }

    public override IRelationTypeWithIsDependency Build()
    {
        var alias = _alias ?? Guid.NewGuid().ToString();
        var name = _name ?? Guid.NewGuid().ToString();
        var parentObjectType = _parentObjectType;
        var childObjectType = _childObjectType;
        var id = _id ?? 0;
        var key = _key ?? Guid.NewGuid();
        var isBidirectional = _isBidirectional ?? false;
        var isDependency = _isDependency ?? false;
        var createDate = _createDate ?? DateTime.Now;
        var updateDate = _updateDate ?? DateTime.Now;
        var deleteDate = _deleteDate;

        return new RelationType(name, alias, isBidirectional, parentObjectType, childObjectType, isDependency)
        {
            Id = id,
            Key = key,
            CreateDate = createDate,
            UpdateDate = updateDate,
            DeleteDate = deleteDate
        };
    }
}
