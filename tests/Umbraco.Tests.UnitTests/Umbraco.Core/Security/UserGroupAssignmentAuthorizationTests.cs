using NUnit.Framework;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Security;

[TestFixture]
public class UserGroupAssignmentAuthorizationTests
{
    [Test]
    public void Returns_Empty_When_No_Groups_Are_Being_Added()
    {
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: ["editor"],
            requestedGroupAliases: ["editor"],
            existingGroupAliases: ["editor"]);

        Assert.IsEmpty(result);
    }

    [Test]
    public void Returns_Empty_When_Added_Groups_Belong_To_Performer()
    {
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: ["editor", "writer"],
            requestedGroupAliases: ["editor", "writer"],
            existingGroupAliases: ["editor"]);

        Assert.IsEmpty(result);
    }

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

    [Test]
    public void Existing_Groups_Are_Not_Flagged_Even_If_Performer_Does_Not_Belong()
    {
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: ["editor"],
            requestedGroupAliases: ["writer", "editor"],
            existingGroupAliases: ["writer"]);

        Assert.IsEmpty(result);
    }

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

    [Test]
    public void Comparison_Is_Case_Insensitive()
    {
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: ["Editor"],
            requestedGroupAliases: ["editor", "Writer"],
            existingGroupAliases: ["WRITER"]);

        Assert.IsEmpty(result);
    }

    [Test]
    public void Returns_Empty_When_No_Groups_Requested()
    {
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: ["editor"],
            requestedGroupAliases: [],
            existingGroupAliases: ["editor", "writer"]);

        Assert.IsEmpty(result);
    }

    [Test]
    public void Returns_Empty_When_Performer_Has_No_Groups_But_Nothing_Added()
    {
        var result = UserGroupAssignmentAuthorization.GetUnauthorizedGroupAssignments(
            performingUserGroupAliases: [],
            requestedGroupAliases: ["editor"],
            existingGroupAliases: ["editor"]);

        Assert.IsEmpty(result);
    }

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
