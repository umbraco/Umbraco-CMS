// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class MemberTypeBuilder
    : ContentTypeBaseBuilder<MemberBuilder, IMemberType>,
        IWithPropertyTypeIdsIncrementingFrom
{
    private readonly Dictionary<string, bool> _memberCanEditProperties = new();
    private readonly Dictionary<string, bool> _memberCanViewProperties = new();
    private readonly List<PropertyGroupBuilder<MemberTypeBuilder>> _propertyGroupBuilders = new();
    private int? _propertyTypeIdsIncrementingFrom;

    public MemberTypeBuilder()
        : base(null)
    {
    }

    public MemberTypeBuilder(MemberBuilder parentBuilder)
        : base(parentBuilder)
    {
    }

    int? IWithPropertyTypeIdsIncrementingFrom.PropertyTypeIdsIncrementingFrom
    {
        get => _propertyTypeIdsIncrementingFrom;
        set => _propertyTypeIdsIncrementingFrom = value;
    }

    public MemberTypeBuilder WithMembershipPropertyGroup()
    {
        var builder = new PropertyGroupBuilder<MemberTypeBuilder>(this)
            .WithId(99)
            .WithName(Constants.Conventions.Member.StandardPropertiesGroupName)
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextArea)
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithAlias(Constants.Conventions.Member.Comments)
            .WithName(Constants.Conventions.Member.CommentsLabel)
            .Done();
        _propertyGroupBuilders.Add(builder);
        return this;
    }

    public MemberTypeBuilder WithMemberCanEditProperty(string alias, bool canEdit)
    {
        _memberCanEditProperties.Add(alias, canEdit);
        return this;
    }

    public MemberTypeBuilder WithMemberCanViewProperty(string alias, bool canView)
    {
        _memberCanViewProperties.Add(alias, canView);
        return this;
    }

    public PropertyGroupBuilder<MemberTypeBuilder> AddPropertyGroup()
    {
        var builder = new PropertyGroupBuilder<MemberTypeBuilder>(this);
        _propertyGroupBuilders.Add(builder);
        return builder;
    }

    public override IMemberType Build()
    {
        var memberType = new MemberType(ShortStringHelper, GetParentId())
        {
            Id = GetId(),
            Key = GetKey(),
            CreateDate = GetCreateDate(),
            UpdateDate = GetUpdateDate(),
            Alias = GetAlias(),
            Name = GetName(),
            Level = GetLevel(),
            Path = GetPath(),
            SortOrder = GetSortOrder(),
            Description = GetDescription(),
            Icon = GetIcon(),
            Thumbnail = GetThumbnail(),
            CreatorId = GetCreatorId(),
            Trashed = GetTrashed(),
            IsContainer = GetIsContainer()
        };

        BuildPropertyGroups(memberType, _propertyGroupBuilders.Select(x => x.Build()));
        BuildPropertyTypeIds(memberType, _propertyTypeIdsIncrementingFrom);

        foreach (var kvp in _memberCanEditProperties)
        {
            memberType.SetMemberCanEditProperty(kvp.Key, kvp.Value);
        }

        foreach (var kvp in _memberCanViewProperties)
        {
            memberType.SetMemberCanViewProperty(kvp.Key, kvp.Value);
        }

        memberType.ResetDirtyProperties(false);

        return memberType;
    }

    public static MemberType CreateSimpleMemberType(string alias = null, string name = null)
    {
        var builder = new MemberTypeBuilder();
        var memberType = builder
            .WithAlias(alias)
            .WithName(name)
            .AddPropertyGroup()
            .WithName("Content")
            .WithSortOrder(1)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithSortOrder(1)
            .Done()
            .AddPropertyType()
            .WithAlias("bodyText")
            .WithName("Body text")
            .WithSortOrder(2)
            .WithDataTypeId(-87)
            .Done()
            .AddPropertyType()
            .WithAlias("author")
            .WithName("Author")
            .WithSortOrder(3)
            .Done()
            .Done()
            .Build();

        // Ensure that nothing is marked as dirty.
        memberType.ResetDirtyProperties(false);

        return (MemberType)memberType;
    }
}
