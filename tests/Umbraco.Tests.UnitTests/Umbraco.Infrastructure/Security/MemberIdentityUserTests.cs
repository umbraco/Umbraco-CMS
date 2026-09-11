// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security;

[TestFixture]
public class MemberIdentityUserTests
{
    private static readonly JsonSerializerOptions _caseInsensitiveOptions = new() { PropertyNameCaseInsensitive = true };

    [Test]
    public void GetProfileData_With_CaseInsensitive_Options_Returns_Typed_Object()
    {
        // Arrange
        var user = new MemberIdentityUser
        {
            ProfileData = """{"name":"Test","age":30}""",
        };

        // Act
        var result = user.GetProfileData<TestProfile>(_caseInsensitiveOptions);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Test"));
        Assert.That(result.Age, Is.EqualTo(30));
    }

    [Test]
    public void GetProfileData_With_Default_Options_Is_Case_Sensitive()
    {
        // Arrange — lowercase JSON keys don't match PascalCase properties without case-insensitive options.
        var user = new MemberIdentityUser
        {
            ProfileData = """{"name":"Test","age":30}""",
        };

        // Act
        var result = user.GetProfileData<TestProfile>();

        // Assert — properties are default because the keys don't match.
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.Null);
        Assert.That(result.Age, Is.EqualTo(0));
    }

    [Test]
    public void GetProfileData_With_Matching_Case_Works_Without_Options()
    {
        // Arrange — PascalCase JSON matches PascalCase properties.
        var user = new MemberIdentityUser
        {
            ProfileData = """{"Name":"Test","Age":30}""",
        };

        // Act
        var result = user.GetProfileData<TestProfile>();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Test"));
        Assert.That(result.Age, Is.EqualTo(30));
    }

    [Test]
    public void GetProfileData_Returns_Null_When_ProfileData_Is_Null()
    {
        // Arrange
        var user = new MemberIdentityUser { ProfileData = null };

        // Act
        var result = user.GetProfileData<TestProfile>();

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetProfileData_Returns_Null_When_ProfileData_Is_Empty()
    {
        // Arrange
        var user = new MemberIdentityUser { ProfileData = string.Empty };

        // Act
        var result = user.GetProfileData<TestProfile>();

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetProfileData_As_Dictionary()
    {
        // Arrange
        var user = new MemberIdentityUser
        {
            ProfileData = """{"favouriteColor":"Green","homeCity":"London"}""",
        };

        // Act — dictionary keys are case-sensitive strings, so no options needed.
        var result = user.GetProfileData<Dictionary<string, string>>();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!["favouriteColor"], Is.EqualTo("Green"));
        Assert.That(result["homeCity"], Is.EqualTo("London"));
    }

    [Test]
    public void ProfileData_Setting_New_Value_Marks_Property_Dirty()
    {
        // Arrange
        var user = new MemberIdentityUser();

        // Act
        user.ProfileData = """{"name":"Test"}""";

        // Assert — dirty tracking is load-bearing for MemberUserStore.UpdateExternalMemberAsync
        // to detect OnExternalLogin callback refreshes and route to the full update path rather
        // than the lightweight login path (which would otherwise lose the ProfileData change).
        Assert.That(user.IsPropertyDirty(nameof(MemberIdentityUser.ProfileData)), Is.True);
    }

    [Test]
    public void ProfileData_Setting_Same_Value_Does_Not_Mark_Property_Dirty()
    {
        // Arrange
        var user = new MemberIdentityUser();
        user.DisableChangeTracking();
        user.ProfileData = """{"name":"Test"}""";
        user.EnableChangeTracking();

        // Act — reassigning the exact same value should not flip the dirty flag.
        user.ProfileData = """{"name":"Test"}""";

        // Assert
        Assert.That(user.IsPropertyDirty(nameof(MemberIdentityUser.ProfileData)), Is.False);
    }

    [Test]
    public void ProfileData_Setting_With_Change_Tracking_Disabled_Does_Not_Mark_Property_Dirty()
    {
        // Arrange — MemberUserStore.MapExternalMemberToIdentityUser hydrates ProfileData from
        // the store inside a DisableChangeTracking/EnableChangeTracking pair so initial load
        // does not appear as a pending change.
        var user = new MemberIdentityUser();
        user.DisableChangeTracking();

        // Act
        user.ProfileData = """{"name":"Test"}""";

        // Assert
        Assert.That(user.IsPropertyDirty(nameof(MemberIdentityUser.ProfileData)), Is.False);
    }

    private class TestProfile
    {
        public string? Name { get; set; }

        public int Age { get; set; }
    }
}
