using NUnit.Framework;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Security;

/// <summary>
/// Contains unit tests that verify the authorization logic for assigning user groups in the Umbraco CMS.
/// </summary>
[TestFixture]
public class UserGroupAssignmentAuthorizationTests
{
    /// <summary>
    /// Tests that no unauthorized group assignments are returned when no groups are being added.
    /// </summary>
    [Test]
    public void Returns_Empty_When_No_Groups_Are_Being_Added()
    {
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: ["editor"],
            requestedGroupAliases: ["editor"],
            existingGroupAliases: ["editor"]);

        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Tests that the method returns an empty collection when the added groups belong to the performing user.
    /// </summary>
    [Test]
    public void Returns_Empty_When_Added_Groups_Belong_To_Performer()
    {
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: ["editor", "writer"],
            requestedGroupAliases: ["editor", "writer"],
            existingGroupAliases: ["editor"]);

        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Tests that unauthorized user groups not belonging to the performing user are correctly identified.
    /// </summary>
    [Test]
    public void Returns_Unauthorized_Groups_Not_Belonging_To_Performer()
    {
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: ["editor"],
            requestedGroupAliases: ["editor", "admin"],
            existingGroupAliases: ["editor"]);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("admin", result[0]);
    }

    /// <summary>
    /// Tests that existing groups are not flagged as unauthorized even if the performing user does not belong to them.
    /// </summary>
    [Test]
    public void Existing_Groups_Are_Not_Flagged_Even_If_Performer_Does_Not_Belong()
    {
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: ["editor"],
            requestedGroupAliases: ["writer", "editor"],
            existingGroupAliases: ["writer"]);

        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Verifies that all unauthorized group assignments are returned when multiple new groups fail authorization.
    /// </summary>
    [Test]
    public void Returns_All_Unauthorized_When_Multiple_New_Groups_Fail()
    {
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: ["editor"],
            requestedGroupAliases: ["editor", "admin", "sensitiveData"],
            existingGroupAliases: []);

        Assert.AreEqual(2, result.Count);
        CollectionAssert.AreEquivalent(new[] { "admin", "sensitiveData" }, result);
    }

    /// <summary>
    /// Tests that the method returns an empty collection when all requested groups are removals.
    /// </summary>
    [Test]
    public void Returns_Empty_When_All_Requested_Groups_Are_Removals()
    {
        // Requesting fewer groups than existing = only removals, no additions
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: ["editor"],
            requestedGroupAliases: ["editor"],
            existingGroupAliases: ["editor", "writer", "admin"]);

        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Tests that the comparison of user group assignments is case insensitive.
    /// </summary>
    [Test]
    public void Comparison_Is_Case_Insensitive()
    {
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: ["Editor"],
            requestedGroupAliases: ["editor", "Writer"],
            existingGroupAliases: ["WRITER"]);

        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Tests that the method returns an empty collection when no groups are requested.
    /// </summary>
    [Test]
    public void Returns_Empty_When_No_Groups_Requested()
    {
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: ["editor"],
            requestedGroupAliases: [],
            existingGroupAliases: ["editor", "writer"]);

        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Tests that the method returns an empty collection when the performer has no groups and no new groups are added.
    /// </summary>
    [Test]
    public void Returns_Empty_When_Performer_Has_No_Groups_But_Nothing_Added()
    {
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: [],
            requestedGroupAliases: ["editor"],
            existingGroupAliases: ["editor"]);

        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Verifies that authorization returns unauthorized when the performing user has no groups and attempts to add groups.
    /// </summary>
    [Test]
    public void Returns_Unauthorized_When_Performer_Has_No_Groups_And_Groups_Added()
    {
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: [],
            requestedGroupAliases: ["editor"],
            existingGroupAliases: []);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("editor", result[0]);
    }
}
