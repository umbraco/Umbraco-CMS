// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class UserTests
{
    [SetUp]
    public void SetUp() => _builder = new UserBuilder();

    private UserBuilder _builder;

    [Test]
    public void Can_Deep_Clone()
    {
        var item = BuildUser();

        var clone = (User)item.DeepClone();

        Assert.AreNotSame(clone, item);
        Assert.AreEqual(clone, item);

        Assert.AreEqual(clone.AllowedSections.Count(), item.AllowedSections.Count());

        // Verify normal properties with reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(item, null));
        }
    }

    [Test]
    public void Can_Serialize_Without_Error()
    {
        var item = BuildUser();

        var json = JsonSerializer.Serialize(item);
        Debug.Print(json);
    }

    private User BuildUser() =>
        _builder
            .WithId(3)
            .WithLogin("username", "test pass")
            .WithName("Test")
            .WithEmail("test@test.com")
            .WithFailedPasswordAttempts(3)
            .WithIsApproved(true)
            .WithIsLockedOut(true)
            .WithComments("comments")
            .WithSessionTimeout(5)
            .WithStartContentIds([3])
            .WithStartMediaIds([8])
            .Build();

    [Test]
    public void UserState_Is_Active_When_Approved_And_Has_Logged_In()
    {
        var user = CreateUser();
        user.IsApproved = true;
        user.IsLockedOut = false;
        user.LastLoginDate = DateTime.UtcNow;

        Assert.AreEqual(UserState.Active, user.UserState);
    }

    [Test]
    public void UserState_Is_Inactive_When_Default_User_Has_Never_Logged_In()
    {
        var user = CreateUser();
        user.IsApproved = true;
        user.IsLockedOut = false;
        user.LastLoginDate = null;

        Assert.AreEqual(UserState.Inactive, user.UserState);
    }

    [Test]
    public void UserState_Is_Disabled_When_Not_Approved()
    {
        var user = CreateUser();
        user.IsApproved = false;
        user.IsLockedOut = false;
        user.LastLoginDate = DateTime.UtcNow;

        Assert.AreEqual(UserState.Disabled, user.UserState);
    }

    [Test]
    public void UserState_Is_LockedOut_When_Locked_Out()
    {
        var user = CreateUser();
        user.IsApproved = true;
        user.IsLockedOut = true;
        user.LastLoginDate = DateTime.UtcNow;

        Assert.AreEqual(UserState.LockedOut, user.UserState);
    }

    [Test]
    public void UserState_Is_Invited_When_Not_Approved_And_Never_Logged_In_With_InvitedDate()
    {
        var user = CreateUser();
        user.IsApproved = false;
        user.IsLockedOut = false;
        user.LastLoginDate = null;
        user.InvitedDate = DateTime.UtcNow;

        Assert.AreEqual(UserState.Invited, user.UserState);
    }

    [Test]
    public void UserState_Is_Active_For_Api_User_That_Has_Never_Logged_In()
    {
        // API users authenticate via client credentials and never set LastLoginDate, so they should be considered
        // Active when approved and not locked out — see https://github.com/umbraco/Umbraco-CMS/issues/22786.
        var user = CreateUser();
        user.Kind = UserKind.Api;
        user.IsApproved = true;
        user.IsLockedOut = false;
        user.LastLoginDate = null;

        Assert.AreEqual(UserState.Active, user.UserState);
    }

    [Test]
    public void UserState_Is_Disabled_For_Api_User_That_Is_Not_Approved()
    {
        var user = CreateUser();
        user.Kind = UserKind.Api;
        user.IsApproved = false;
        user.IsLockedOut = false;
        user.LastLoginDate = null;

        Assert.AreEqual(UserState.Disabled, user.UserState);
    }

    [Test]
    public void UserState_Is_LockedOut_For_Api_User_That_Is_Locked_Out()
    {
        var user = CreateUser();
        user.Kind = UserKind.Api;
        user.IsApproved = true;
        user.IsLockedOut = true;
        user.LastLoginDate = null;

        Assert.AreEqual(UserState.LockedOut, user.UserState);
    }

    private static User CreateUser() =>
        new(new GlobalSettings(), "Test", "test@test.com", "test", "password");
}
