using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models;

[TestFixture]
public class SecuritySettingsTests
{
    [Test]
    public void GetUserAllowConcurrentLogins_Returns_Global_False_When_Null()
    {
        var settings = new SecuritySettings
        {
            AllowConcurrentLogins = false,
            UserAllowConcurrentLogins = null,
        };

        Assert.That(settings.GetUserAllowConcurrentLogins(), Is.False);
    }

    [Test]
    public void GetUserAllowConcurrentLogins_Returns_Global_True_When_Null()
    {
        var settings = new SecuritySettings
        {
            AllowConcurrentLogins = true,
            UserAllowConcurrentLogins = null,
        };

        Assert.That(settings.GetUserAllowConcurrentLogins(), Is.True);
    }

    [Test]
    public void GetUserAllowConcurrentLogins_Returns_Explicit_True_Overriding_Global_False()
    {
        var settings = new SecuritySettings
        {
            AllowConcurrentLogins = false,
            UserAllowConcurrentLogins = true,
        };

        Assert.That(settings.GetUserAllowConcurrentLogins(), Is.True);
    }

    [Test]
    public void GetUserAllowConcurrentLogins_Returns_Explicit_False_Overriding_Global_True()
    {
        var settings = new SecuritySettings
        {
            AllowConcurrentLogins = true,
            UserAllowConcurrentLogins = false,
        };

        Assert.That(settings.GetUserAllowConcurrentLogins(), Is.False);
    }

    [Test]
    public void GetMemberAllowConcurrentLogins_Returns_Global_False_When_Null()
    {
        var settings = new SecuritySettings
        {
            AllowConcurrentLogins = false,
            MemberAllowConcurrentLogins = null,
        };

        Assert.That(settings.GetMemberAllowConcurrentLogins(), Is.False);
    }

    [Test]
    public void GetMemberAllowConcurrentLogins_Returns_Global_True_When_Null()
    {
        var settings = new SecuritySettings
        {
            AllowConcurrentLogins = true,
            MemberAllowConcurrentLogins = null,
        };

        Assert.That(settings.GetMemberAllowConcurrentLogins(), Is.True);
    }

    [Test]
    public void GetMemberAllowConcurrentLogins_Returns_Explicit_True_Overriding_Global_False()
    {
        var settings = new SecuritySettings
        {
            AllowConcurrentLogins = false,
            MemberAllowConcurrentLogins = true,
        };

        Assert.That(settings.GetMemberAllowConcurrentLogins(), Is.True);
    }

    [Test]
    public void GetMemberAllowConcurrentLogins_Returns_Explicit_False_Overriding_Global_True()
    {
        var settings = new SecuritySettings
        {
            AllowConcurrentLogins = true,
            MemberAllowConcurrentLogins = false,
        };

        Assert.That(settings.GetMemberAllowConcurrentLogins(), Is.False);
    }

    [Test]
    public void Independent_Overrides_User_True_Member_False()
    {
        var settings = new SecuritySettings
        {
            AllowConcurrentLogins = true,
            UserAllowConcurrentLogins = true,
            MemberAllowConcurrentLogins = false,
        };

        Assert.That(settings.GetUserAllowConcurrentLogins(), Is.True);
        Assert.That(settings.GetMemberAllowConcurrentLogins(), Is.False);
    }

    [Test]
    public void CallbackPathName_Defaults_To_Umbraco()
    {
        var settings = new SecuritySettings();

        Assert.That(settings.CallbackPathName, Is.EqualTo("/umbraco"));
    }

    [Test]
    public void AuthorizeCallbackPathName_Reads_Through_To_CallbackPathName()
    {
        var settings = new SecuritySettings();

#pragma warning disable CS0618 // Type or member is obsolete
        settings.AuthorizeCallbackPathName = "/custom";
        Assert.That(settings.CallbackPathName, Is.EqualTo("/custom"));

        settings.CallbackPathName = "/other";
        Assert.That(settings.AuthorizeCallbackPathName, Is.EqualTo("/other"));
#pragma warning restore CS0618
    }

    [Test]
    public void Effective_Logout_And_Error_Default_From_CallbackPathName()
    {
        var settings = new SecuritySettings();

        Assert.That(settings.GetEffectiveLogoutPathName(), Is.EqualTo("/umbraco/logout"));
        Assert.That(settings.GetEffectiveErrorPathName(), Is.EqualTo("/umbraco/error"));
    }

    [Test]
    public void Effective_Logout_Prefers_Explicit_Value_Over_Derived()
    {
        var settings = new SecuritySettings();

#pragma warning disable CS0618 // Type or member is obsolete
        settings.AuthorizeCallbackLogoutPathName = "/backoffice/bye";
#pragma warning restore CS0618

        Assert.That(settings.GetEffectiveLogoutPathName(), Is.EqualTo("/backoffice/bye"));
    }
}
