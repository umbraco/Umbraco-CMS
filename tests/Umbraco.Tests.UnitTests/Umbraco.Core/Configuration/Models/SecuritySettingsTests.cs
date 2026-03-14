using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models;

/// <summary>
/// Contains unit tests for the <see cref="SecuritySettings"/> configuration model in Umbraco CMS.
/// </summary>
[TestFixture]
public class SecuritySettingsTests
{
    /// <summary>
    /// Verifies that GetUserAllowConcurrentLogins returns false when UserAllowConcurrentLogins is null and AllowConcurrentLogins is set to false.
    /// </summary>
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

    /// <summary>
    /// Tests that GetUserAllowConcurrentLogins returns the global AllowConcurrentLogins value when UserAllowConcurrentLogins is null.
    /// </summary>
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

    /// <summary>
    /// Tests that GetUserAllowConcurrentLogins returns true when UserAllowConcurrentLogins is explicitly set to true,
    /// overriding the global AllowConcurrentLogins setting set to false.
    /// </summary>
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

    /// <summary>
    /// Tests that GetUserAllowConcurrentLogins returns false when UserAllowConcurrentLogins is explicitly set to false,
    /// overriding the global AllowConcurrentLogins setting set to true.
    /// </summary>
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

    /// <summary>
    /// Tests that GetMemberAllowConcurrentLogins returns false when MemberAllowConcurrentLogins is null and the global AllowConcurrentLogins is false.
    /// </summary>
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

    /// <summary>
    /// Tests that GetMemberAllowConcurrentLogins returns true when MemberAllowConcurrentLogins is null and the global AllowConcurrentLogins is true.
    /// </summary>
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

    /// <summary>
    /// Tests that GetMemberAllowConcurrentLogins returns true when MemberAllowConcurrentLogins is explicitly set to true,
    /// overriding the global AllowConcurrentLogins setting which is false.
    /// </summary>
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

    /// <summary>
    /// Tests that GetMemberAllowConcurrentLogins returns false when MemberAllowConcurrentLogins is explicitly set to false, overriding the global AllowConcurrentLogins set to true.
    /// </summary>
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

    /// <summary>
    /// Tests that independent settings override correctly when UserAllowConcurrentLogins is true and MemberAllowConcurrentLogins is false.
    /// </summary>
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
