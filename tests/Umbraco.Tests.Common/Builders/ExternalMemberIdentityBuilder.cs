// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ExternalMemberIdentityBuilder
{
    private string _email = "test@example.com";
    private string _userName = "test@example.com";
    private string? _name = "Test Member";
    private bool _isApproved = true;
    private bool _isLockedOut;
    private Guid? _key;
    private string? _profileData;

    public ExternalMemberIdentityBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public ExternalMemberIdentityBuilder WithUserName(string userName)
    {
        _userName = userName;
        return this;
    }

    public ExternalMemberIdentityBuilder WithName(string? name)
    {
        _name = name;
        return this;
    }

    public ExternalMemberIdentityBuilder WithIsApproved(bool isApproved)
    {
        _isApproved = isApproved;
        return this;
    }

    public ExternalMemberIdentityBuilder WithIsLockedOut(bool isLockedOut)
    {
        _isLockedOut = isLockedOut;
        return this;
    }

    public ExternalMemberIdentityBuilder WithKey(Guid key)
    {
        _key = key;
        return this;
    }

    public ExternalMemberIdentityBuilder WithProfileData(string? profileData)
    {
        _profileData = profileData;
        return this;
    }

    public ExternalMemberIdentity Build() =>
        new()
        {
            Key = _key ?? Guid.NewGuid(),
            Email = _email,
            UserName = _userName,
            Name = _name,
            IsApproved = _isApproved,
            IsLockedOut = _isLockedOut,
            CreateDate = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString(),
            ProfileData = _profileData,
        };

    public static ExternalMemberIdentity CreateSimple(string email = "test@example.com", string? name = null) =>
        new ExternalMemberIdentityBuilder()
            .WithEmail(email)
            .WithUserName(email)
            .WithName(name ?? email)
            .Build();
}
