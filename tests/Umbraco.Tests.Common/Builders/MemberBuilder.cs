// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class MemberBuilder
    : BuilderBase<Member>,
        IBuildContentTypes,
        IWithIdBuilder,
        IWithKeyBuilder,
        IWithCreatorIdBuilder,
        IWithCreateDateBuilder,
        IWithUpdateDateBuilder,
        IWithNameBuilder,
        IWithTrashedBuilder,
        IWithLevelBuilder,
        IWithPathBuilder,
        IWithSortOrderBuilder,
        IAccountBuilder
{
    private GenericDictionaryBuilder<MemberBuilder, string, object> _additionalDataBuilder;
    private DateTime? _createDate;
    private int? _creatorId;
    private string _email;
    private int? _failedPasswordAttempts;

    private int? _id;
    private bool? _isApproved;
    private bool? _isLockedOut;
    private Guid? _key;
    private DateTime? _lastLockoutDate;
    private DateTime? _lastLoginDate;
    private DateTime? _lastPasswordChangeDate;
    private int? _level;
    private GenericCollectionBuilder<MemberBuilder, string> _memberGroupsBuilder;
    private IMemberType _memberType;
    private MemberTypeBuilder _memberTypeBuilder;
    private string _name;
    private string _passwordConfig;
    private string _path;
    private GenericDictionaryBuilder<MemberBuilder, string, object> _propertyDataBuilder;
    private int? _propertyIdsIncrementingFrom;
    private string _rawPasswordValue;
    private int? _sortOrder;
    private bool? _trashed;
    private DateTime? _updateDate;
    private string _username;

    string IWithLoginBuilder.Username
    {
        get => _username;
        set => _username = value;
    }

    string IWithLoginBuilder.RawPasswordValue
    {
        get => _rawPasswordValue;
        set => _rawPasswordValue = value;
    }

    string IWithLoginBuilder.PasswordConfig
    {
        get => _passwordConfig;
        set => _passwordConfig = value;
    }

    string IWithEmailBuilder.Email
    {
        get => _email;
        set => _email = value;
    }

    int? IWithFailedPasswordAttemptsBuilder.FailedPasswordAttempts
    {
        get => _failedPasswordAttempts;
        set => _failedPasswordAttempts = value;
    }

    bool? IWithIsApprovedBuilder.IsApproved
    {
        get => _isApproved;
        set => _isApproved = value;
    }

    bool? IWithIsLockedOutBuilder.IsLockedOut
    {
        get => _isLockedOut;
        set => _isLockedOut = value;
    }

    DateTime? IWithIsLockedOutBuilder.LastLockoutDate
    {
        get => _lastLockoutDate;
        set => _lastLockoutDate = value;
    }

    DateTime? IWithLastLoginDateBuilder.LastLoginDate
    {
        get => _lastLoginDate;
        set => _lastLoginDate = value;
    }

    DateTime? IWithLastPasswordChangeDateBuilder.LastPasswordChangeDate
    {
        get => _lastPasswordChangeDate;
        set => _lastPasswordChangeDate = value;
    }

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

    int? IWithLevelBuilder.Level
    {
        get => _level;
        set => _level = value;
    }

    string IWithNameBuilder.Name
    {
        get => _name;
        set => _name = value;
    }

    string IWithPathBuilder.Path
    {
        get => _path;
        set => _path = value;
    }

    int? IWithSortOrderBuilder.SortOrder
    {
        get => _sortOrder;
        set => _sortOrder = value;
    }

    bool? IWithTrashedBuilder.Trashed
    {
        get => _trashed;
        set => _trashed = value;
    }

    DateTime? IWithUpdateDateBuilder.UpdateDate
    {
        get => _updateDate;
        set => _updateDate = value;
    }

    public MemberBuilder WithPropertyIdsIncrementingFrom(int propertyIdsIncrementingFrom)
    {
        _propertyIdsIncrementingFrom = propertyIdsIncrementingFrom;
        return this;
    }

    public MemberTypeBuilder AddMemberType()
    {
        _memberType = null;
        var builder = new MemberTypeBuilder(this);
        _memberTypeBuilder = builder;
        return builder;
    }

    public MemberBuilder WithMemberType(IMemberType memberType)
    {
        _memberTypeBuilder = null;
        _memberType = memberType;

        return this;
    }

    public GenericCollectionBuilder<MemberBuilder, string> AddMemberGroups()
    {
        var builder = new GenericCollectionBuilder<MemberBuilder, string>(this);
        _memberGroupsBuilder = builder;
        return builder;
    }

    public GenericDictionaryBuilder<MemberBuilder, string, object> AddAdditionalData()
    {
        var builder = new GenericDictionaryBuilder<MemberBuilder, string, object>(this);
        _additionalDataBuilder = builder;
        return builder;
    }

    public GenericDictionaryBuilder<MemberBuilder, string, object> AddPropertyData()
    {
        var builder = new GenericDictionaryBuilder<MemberBuilder, string, object>(this);
        _propertyDataBuilder = builder;
        return builder;
    }

    public override Member Build()
    {
        var id = _id ?? 0;
        var key = _key ?? Guid.NewGuid();
        var createDate = _createDate ?? DateTime.Now;
        var updateDate = _updateDate ?? DateTime.Now;
        var name = _name ?? Guid.NewGuid().ToString();
        var creatorId = _creatorId ?? 0;
        var level = _level ?? 1;
        var path = _path ?? $"-1,{id}";
        var sortOrder = _sortOrder ?? 0;
        var trashed = _trashed ?? false;
        var username = _username ?? string.Empty;
        var email = _email ?? string.Empty;
        var rawPasswordValue = _rawPasswordValue ?? string.Empty;
        var failedPasswordAttempts = _failedPasswordAttempts ?? 0;
        var isApproved = _isApproved ?? false;
        var isLockedOut = _isLockedOut ?? false;
        var lastLockoutDate = _lastLockoutDate ?? DateTime.Now;
        var lastLoginDate = _lastLoginDate ?? DateTime.Now;
        var lastPasswordChangeDate = _lastPasswordChangeDate ?? DateTime.Now;
        var passwordConfig = _passwordConfig ?? "{\"hashAlgorithm\":\"PBKDF2.ASPNETCORE.V3\"}";

        if (_memberTypeBuilder is null && _memberType is null)
        {
            throw new InvalidOperationException(
                "A member cannot be constructed without providing a member type. Use AddMemberType() or WithMemberType().");
        }

        var memberType = _memberType ?? _memberTypeBuilder.Build();

        var member = new Member(name, email, username, rawPasswordValue, memberType)
        {
            Id = id,
            Key = key,
            CreateDate = createDate,
            UpdateDate = updateDate,
            CreatorId = creatorId,
            Level = level,
            Path = path,
            SortOrder = sortOrder,
            Trashed = trashed,
            PasswordConfiguration = passwordConfig
        };

        if (_propertyIdsIncrementingFrom.HasValue)
        {
            var i = _propertyIdsIncrementingFrom.Value;
            foreach (var property in member.Properties)
            {
                property.Id = ++i;
            }
        }

        member.FailedPasswordAttempts = failedPasswordAttempts;
        member.IsApproved = isApproved;
        member.IsLockedOut = isLockedOut;
        member.LastLockoutDate = lastLockoutDate;
        member.LastLoginDate = lastLoginDate;
        member.LastPasswordChangeDate = lastPasswordChangeDate;

        if (_memberGroupsBuilder != null)
        {
            member.Groups = _memberGroupsBuilder.Build();
        }

        if (_additionalDataBuilder != null)
        {
            var additionalData = _additionalDataBuilder.Build();
            foreach (var kvp in additionalData)
            {
                member.AdditionalData.Add(kvp.Key, kvp.Value);
            }
        }

        if (_propertyDataBuilder != null)
        {
            var propertyData = _propertyDataBuilder.Build();
            foreach (var kvp in propertyData)
            {
                member.SetValue(kvp.Key, kvp.Value);
            }

            member.ResetDirtyProperties(false);
        }

        return member;
    }

    public static IEnumerable<IMember> CreateSimpleMembers(IMemberType memberType, int amount)
    {
        var list = new List<IMember>();

        for (var i = 0; i < amount; i++)
        {
            var name = "Member No-" + i;

            var builder = new MemberBuilder()
                .WithMemberType(memberType)
                .WithName(name)
                .WithEmail("test" + i + "@test.com")
                .WithLogin("test" + i, "test" + i);

            builder = builder
                .AddPropertyData()
                .WithKeyValue("title", name + " member" + i)
                .WithKeyValue("bodyText", "This is a subpage" + i)
                .WithKeyValue("author", "John Doe" + i)
                .Done();

            list.Add(builder.Build());
        }

        return list;
    }

    public static Member CreateSimpleMember(IMemberType memberType, string name, string email, string password, string username, Guid? key = null)
    {
        var builder = new MemberBuilder()
            .WithMemberType(memberType)
            .WithName(name)
            .WithEmail(email)
            .WithIsApproved(true)
            .WithLogin(username, password);

        if (key.HasValue)
        {
            builder = builder.WithKey(key.Value);
        }

        builder = builder
            .AddPropertyData()
            .WithKeyValue("title", name + " member")
            .WithKeyValue("bodyText", "Member profile")
            .WithKeyValue("author", "John Doe")
            .Done();

        return builder.Build();
    }

    public static IEnumerable<IMember> CreateMultipleSimpleMembers(IMemberType memberType, int amount, Action<int, IMember> onCreating = null)
    {
        var list = new List<IMember>();

        for (var i = 0; i < amount; i++)
        {
            var name = "Member No-" + i;
            var member = new MemberBuilder()
                .WithMemberType(memberType)
                .WithName(name)
                .WithEmail("test" + i + "@test.com")
                .WithLogin("test" + i, "test" + i)
                .AddPropertyData()
                .WithKeyValue("title", name + " member" + i)
                .WithKeyValue("bodyText", "Member profile" + i)
                .WithKeyValue("author", "John Doe" + i)
                .Done()
                .Build();

            onCreating?.Invoke(i, member);

            list.Add(member);
        }

        return list;
    }
}
