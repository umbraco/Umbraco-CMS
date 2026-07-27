using Microsoft.AspNetCore.Http;
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

    // The obsolete property used to hold the OAuth callback path (e.g. "/umbraco/oauth_complete").
    // Honouring it would silently redefine where the back office is served, so it now only reads.
    [Test]
    public void AuthorizeCallbackPathName_Reads_Through_But_Setting_It_Has_No_Effect()
    {
        var settings = new SecuritySettings { CallbackPathName = "/other" };

#pragma warning disable CS0618 // Type or member is obsolete
        Assert.That(settings.AuthorizeCallbackPathName, Is.EqualTo("/other"));

        settings.AuthorizeCallbackPathName = "/umbraco/oauth_complete";

        Assert.That(settings.CallbackPathName, Is.EqualTo("/other"));
#pragma warning restore CS0618
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

    // Logout and error are client routes, so they have to stay in step with CallbackPathName. Pointing
    // them elsewhere produced a path the client could not route, so the legacy setters are now inert.
    [Test]
    public void Setting_The_Obsolete_Logout_And_Error_Paths_Has_No_Effect()
    {
        var settings = new SecuritySettings { CallbackPathName = "/" };

#pragma warning disable CS0618 // Type or member is obsolete
        settings.AuthorizeCallbackLogoutPathName = "/backoffice/bye";
        settings.AuthorizeCallbackErrorPathName = "/backoffice/oops";
#pragma warning restore CS0618

        Assert.Multiple(() =>
        {
            Assert.That(settings.GetEffectiveLogoutPathName(), Is.EqualTo("/logout"));
            Assert.That(settings.GetEffectiveErrorPathName(), Is.EqualTo("/error"));
        });
    }

    // ConfigureBackOfficeCookieOptions casts straight from CookieSameSiteMode to SameSiteMode, so the
    // two enums have to keep the same numeric values. Renumbering either one would silently hand the
    // cookie a different SameSite policy than was configured.
    [TestCase(CookieSameSiteMode.Unspecified, SameSiteMode.Unspecified)]
    [TestCase(CookieSameSiteMode.None, SameSiteMode.None)]
    [TestCase(CookieSameSiteMode.Lax, SameSiteMode.Lax)]
    [TestCase(CookieSameSiteMode.Strict, SameSiteMode.Strict)]
    public void CookieSameSiteMode_Matches_AspNetCore_SameSiteMode(CookieSameSiteMode ours, SameSiteMode theirs)
        => Assert.That((int)ours, Is.EqualTo((int)theirs));

    [Test]
    public void CookieSameSiteMode_Covers_Every_AspNetCore_SameSiteMode()
        => Assert.That(
            Enum.GetValues<CookieSameSiteMode>().Select(x => (int)x),
            Is.EquivalentTo(Enum.GetValues<SameSiteMode>().Select(x => (int)x)));

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

        Assert.That(settings.AuthCookieSameSite, Is.EqualTo(CookieSameSiteMode.None));
    }
}
