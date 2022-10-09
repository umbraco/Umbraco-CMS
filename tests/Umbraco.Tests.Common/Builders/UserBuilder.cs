// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Common.Builders;

public class UserBuilder : UserBuilder<object>
{
    public UserBuilder()
        : base(null)
    {
    }
}

public class UserBuilder<TParent>
    : ChildBuilderBase<TParent, User>,
        IWithIdBuilder,
        IWithKeyBuilder,
        IWithCreateDateBuilder,
        IWithUpdateDateBuilder,
        IWithNameBuilder,
        IAccountBuilder
{
    private readonly List<UserGroupBuilder<UserBuilder<TParent>>> _userGroupBuilders = new();
    private string _comments;
    private DateTime? _createDate;
    private string _defaultLang;
    private string _email;
    private int? _failedPasswordAttempts;
    private int? _id;
    private bool? _isApproved;
    private bool? _isLockedOut;
    private Guid? _key;
    private string _language;
    private DateTime? _lastLockoutDate;
    private DateTime? _lastLoginDate;
    private DateTime? _lastPasswordChangeDate;
    private string _name;
    private string _rawPasswordValue;
    private int? _sessionTimeout;
    private int[] _startContentIds;
    private int[] _startMediaIds;
    private string _suffix = string.Empty;
    private DateTime? _updateDate;
    private string _username;

    public UserBuilder(TParent parentBuilder)
        : base(parentBuilder)
    {
    }

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

    string IWithLoginBuilder.PasswordConfig { get; set; }

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

    public UserBuilder<TParent> WithDefaultUILanguage(string defaultLang)
    {
        _defaultLang = defaultLang;
        return this;
    }

    public UserBuilder<TParent> WithLanguage(string language)
    {
        _language = language;
        return this;
    }

    public UserBuilder<TParent> WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public UserBuilder<TParent> WithComments(string comments)
    {
        _comments = comments;
        return this;
    }

    public UserBuilder<TParent> WithSessionTimeout(int sessionTimeout)
    {
        _sessionTimeout = sessionTimeout;
        return this;
    }

    public UserBuilder<TParent> WithStartContentId(int startContentId)
    {
        _startContentIds = new[] { startContentId };
        return this;
    }

    public UserBuilder<TParent> WithStartContentIds(int[] startContentIds)
    {
        _startContentIds = startContentIds;
        return this;
    }

    public UserBuilder<TParent> WithStartMediaId(int startMediaId)
    {
        _startMediaIds = new[] { startMediaId };
        return this;
    }

    public UserBuilder<TParent> WithStartMediaIds(int[] startMediaIds)
    {
        _startMediaIds = startMediaIds;
        return this;
    }

    public UserBuilder<TParent> WithSuffix(string suffix)
    {
        _suffix = suffix;
        return this;
    }

    public UserGroupBuilder<UserBuilder<TParent>> AddUserGroup()
    {
        var builder = new UserGroupBuilder<UserBuilder<TParent>>(this);
        _userGroupBuilders.Add(builder);
        return builder;
    }

    public override User Build()
    {
        var id = _id ?? 0;
        var defaultLang = _defaultLang ?? "en";
        var globalSettings = new GlobalSettings { DefaultUILanguage = defaultLang };
        var key = _key ?? Guid.NewGuid();
        var createDate = _createDate ?? DateTime.Now;
        var updateDate = _updateDate ?? DateTime.Now;
        var name = _name ?? "TestUser" + _suffix;
        var language = _language ?? globalSettings.DefaultUILanguage;
        var username = _username ?? "TestUser" + _suffix;
        var email = _email ?? "test" + _suffix + "@test.com";
        var rawPasswordValue = _rawPasswordValue ?? "abcdefghijklmnopqrstuvwxyz";
        var failedPasswordAttempts = _failedPasswordAttempts ?? 0;
        var isApproved = _isApproved ?? false;
        var isLockedOut = _isLockedOut ?? false;
        var lastLockoutDate = _lastLockoutDate ?? DateTime.Now;
        var lastLoginDate = _lastLoginDate ?? DateTime.Now;
        var lastPasswordChangeDate = _lastPasswordChangeDate ?? DateTime.Now;
        var comments = _comments ?? string.Empty;
        var sessionTimeout = _sessionTimeout ?? 0;
        var startContentIds = _startContentIds ?? new[] { -1 };
        var startMediaIds = _startMediaIds ?? new[] { -1 };
        var groups = _userGroupBuilders.Select(x => x.Build());

        var result = new User(
            globalSettings,
            name,
            email,
            username,
            rawPasswordValue)
        {
            Id = id,
            Key = key,
            CreateDate = createDate,
            UpdateDate = updateDate,
            Language = language,
            FailedPasswordAttempts = failedPasswordAttempts,
            IsApproved = isApproved,
            IsLockedOut = isLockedOut,
            LastLockoutDate = lastLockoutDate,
            LastLoginDate = lastLoginDate,
            LastPasswordChangeDate = lastPasswordChangeDate,
            Comments = comments,
            SessionTimeout = sessionTimeout,
            StartContentIds = startContentIds,
            StartMediaIds = startMediaIds
        };
        foreach (var readOnlyUserGroup in groups)
        {
            result.AddGroup(readOnlyUserGroup.ToReadOnlyGroup());
        }

        return result;
    }

    public static IEnumerable<IUser> CreateMulipleUsers(int amount, Action<int, IUser> onCreating = null)
    {
        var list = new List<IUser>();

        for (var i = 0; i < amount; i++)
        {
            var name = "User No-" + i;
            var user = new UserBuilder()
                .WithName(name)
                .WithEmail("test" + i + "@test.com")
                .WithLogin("test" + i, "test" + i)
                .Build();

            onCreating?.Invoke(i, user);

            user.ResetDirtyProperties(false);

            list.Add(user);
        }

        return list;
    }

    public static User CreateUser(string suffix = "") =>
        new UserBuilder()
            .WithIsApproved(true)
            .WithName("TestUser" + suffix)
            .WithLogin("TestUser" + suffix, "testing")
            .WithEmail("test" + suffix + "@test.com")
            .Build();
}
