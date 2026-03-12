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
}
