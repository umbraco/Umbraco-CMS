using Microsoft.Extensions.Configuration;
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
    public void Effective_Logout_And_Error_Default_From_CallbackPathName()
    {
        var settings = new SecuritySettings();

        Assert.That(settings.GetEffectiveLogoutPathName(), Is.EqualTo("/umbraco/logout"));
        Assert.That(settings.GetEffectiveErrorPathName(), Is.EqualTo("/umbraco/error"));
    }

    // Binding is the path that matters in production (AddUmbracoOptions does a plain Bind), and it
    // behaves differently from setting the properties directly: the binder round-trips every property
    // through get-then-set, so a getter that synthesises a default writes that default into the
    // backing field and makes the "unset" state unobservable.
    [Test]
    public void Effective_Logout_And_Error_Derive_From_CallbackPathName_When_Bound_From_Configuration()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Umbraco:CMS:Security:CallbackPathName"] = "/",
            })
            .Build();

        var settings = new SecuritySettings();
        configuration.GetSection("Umbraco:CMS:Security").Bind(settings);

        Assert.Multiple(() =>
        {
            Assert.That(settings.CallbackPathName, Is.EqualTo("/"));
            Assert.That(settings.GetEffectiveLogoutPathName(), Is.EqualTo("/logout"));
            Assert.That(settings.GetEffectiveErrorPathName(), Is.EqualTo("/error"));
        });
    }

    [Test]
    public void AuthCookieSameSite_Binds_From_Configuration_By_Name()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Umbraco:CMS:Security:AuthCookieSameSite"] = "None",
            })
            .Build();

        var settings = new SecuritySettings();
        configuration.GetSection("Umbraco:CMS:Security").Bind(settings);

        Assert.That(settings.AuthCookieSameSite, Is.EqualTo("None"));
    }
}
