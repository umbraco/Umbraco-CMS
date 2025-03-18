// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class UserGroupBuilder : UserGroupBuilder<object>
{
    public UserGroupBuilder()
        : base(null)
    {
    }
}

public class UserGroupBuilder<TParent>
    : ChildBuilderBase<TParent, IUserGroup>,
        IWithIdBuilder,
        IWithKeyBuilder,
        IWithIconBuilder,
        IWithAliasBuilder,
        IWithNameBuilder
{
    private string _alias;
    private IEnumerable<string> _allowedSections = Enumerable.Empty<string>();
    private IEnumerable<int> _allowedLanguages = Enumerable.Empty<int>();
    private IEnumerable<IGranularPermission> _granularPermissions = Enumerable.Empty<IGranularPermission>();
    private string _icon;
    private int? _id;
    private Guid? _key;
    private string _name;
    private ISet<string> _permissions = new HashSet<string>();
    private int? _startContentId;
    private int? _startMediaId;
    private string _suffix;
    private int? _userCount;

    public UserGroupBuilder(TParent parentBuilder)
        : base(parentBuilder)
    {
    }

    string IWithAliasBuilder.Alias
    {
        get => _alias;
        set => _alias = value;
    }

    string IWithIconBuilder.Icon
    {
        get => _icon;
        set => _icon = value;
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

    /// <summary>
    ///     Will suffix the name, email and username for testing.
    /// </summary>
    /// <param name="suffix">Suffix to add to user group properties.</param>
    /// <returns>Current builder instance.</returns>
    public UserGroupBuilder<TParent> WithSuffix(string suffix)
    {
        _suffix = suffix;
        return this;
    }

    public UserGroupBuilder<TParent> WithUserCount(int userCount)
    {
        _userCount = userCount;
        return this;
    }

    public UserGroupBuilder<TParent> WithPermissions(ISet<string> permissions)
    {
        _permissions = permissions;
        return this;
    }

    public UserGroupBuilder<TParent> WithAllowedSections(IList<string> allowedSections)
    {
        _allowedSections = allowedSections;
        return this;
    }

    public UserGroupBuilder<TParent> WithAllowedLanguages(IList<int> allowedLanguages)
    {
        _allowedLanguages = allowedLanguages;
        return this;
    }

    public UserGroupBuilder<TParent> WithGranularPermissions(IList<IGranularPermission> granularPermissions)
    {
        _granularPermissions = granularPermissions;
        return this;
    }

    public UserGroupBuilder<TParent> WithStartContentId(int startContentId)
    {
        _startContentId = startContentId;
        return this;
    }

    public UserGroupBuilder<TParent> WithStartMediaId(int startMediaId)
    {
        _startMediaId = startMediaId;
        return this;
    }

    public IReadOnlyUserGroup BuildReadOnly(IUserGroup userGroup) =>
        Mock.Of<IReadOnlyUserGroup>(x =>
            x.Permissions == userGroup.Permissions &&
            x.Alias == userGroup.Alias &&
            x.Icon == userGroup.Icon &&
            x.Name == userGroup.Name &&
            x.StartContentId == userGroup.StartContentId &&
            x.StartMediaId == userGroup.StartMediaId &&
            x.AllowedSections == userGroup.AllowedSections &&
            x.Id == userGroup.Id &&
            x.Key == userGroup.Key);

    public override IUserGroup Build()
    {
        var id = _id ?? 0;
        var key = _key ?? Guid.NewGuid();
        var name = _name ?? "TestUserGroup" + _suffix;
        var alias = _alias ?? "testUserGroup" + _suffix;
        var userCount = _userCount ?? 0;
        var startContentId = _startContentId ?? -1;
        var startMediaId = _startMediaId ?? -1;
        var icon = _icon ?? "icon-group";

        var shortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

        var userGroup = new UserGroup(shortStringHelper, userCount, alias, name, icon)
        {
            Id = id,
            Key = key,
            StartContentId = startContentId,
            StartMediaId = startMediaId,
            Permissions = _permissions,
        };

        foreach (var section in _allowedSections)
        {
            userGroup.AddAllowedSection(section);
        }

        foreach (var language in _allowedLanguages)
        {
            userGroup.AddAllowedLanguage(language);
        }

        foreach (var permission in _granularPermissions)
        {
            userGroup.GranularPermissions.Add(permission);
        }

        return userGroup;
    }

    public static UserGroup CreateUserGroup(
        string alias = "testGroup",
        string name = "Test Group",
        string suffix = "",
        ISet<string>? permissions = null,
        string[] allowedSections = null) =>
        (UserGroup)new UserGroupBuilder()
            .WithAlias(alias + suffix)
            .WithName(name + suffix)
            .WithPermissions(permissions ?? new[] { "A", "B", "C" }.ToHashSet())
            .WithAllowedSections(allowedSections ?? ["content", "media"])
            .Build();
}
