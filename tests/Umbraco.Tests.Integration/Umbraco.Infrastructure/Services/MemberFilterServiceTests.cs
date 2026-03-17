// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
internal sealed class MemberFilterServiceTests : UmbracoIntegrationTest
{
    private IMemberFilterService MemberFilterService => GetRequiredService<IMemberFilterService>();

    private IExternalMemberService ExternalMemberService => GetRequiredService<IExternalMemberService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    [Test]
    public async Task Filter_Returns_Content_Members()
    {
        // Arrange
        await CreateContentMemberAsync("content@test.com", "content-user");

        // Act
        PagedModel<MemberFilterItem> result = await MemberFilterService.FilterAsync(new MemberFilter());

        // Assert
        Assert.AreEqual(1, result.Total);
        var item = result.Items.First();
        Assert.AreEqual("content@test.com", item.Email);
        Assert.IsFalse(item.IsExternalOnly);
        Assert.AreEqual(MemberKind.Default, item.Kind);
        Assert.IsNotNull(item.MemberTypeKey);
        Assert.AreNotEqual(Guid.Empty, item.MemberTypeKey);
        Assert.IsNotNull(item.MemberTypeIcon);
    }

    [Test]
    public async Task Filter_Returns_External_Members()
    {
        // Arrange
        await CreateExternalMemberAsync("external@test.com", "external-user");

        // Act
        PagedModel<MemberFilterItem> result = await MemberFilterService.FilterAsync(new MemberFilter());

        // Assert
        Assert.AreEqual(1, result.Total);
        var item = result.Items.First();
        Assert.AreEqual("external@test.com", item.Email);
        Assert.IsTrue(item.IsExternalOnly);
        Assert.AreEqual(MemberKind.ExternalOnly, item.Kind);
        Assert.IsNull(item.MemberTypeKey);
        Assert.IsNull(item.MemberTypeIcon);
    }

    [Test]
    public async Task Filter_Returns_Both_Content_And_External_Members()
    {
        // Arrange
        await CreateContentMemberAsync("content@test.com", "content-user");
        await CreateExternalMemberAsync("external@test.com", "external-user");

        // Act
        PagedModel<MemberFilterItem> result = await MemberFilterService.FilterAsync(new MemberFilter());

        // Assert
        Assert.AreEqual(2, result.Total);
        Assert.IsTrue(result.Items.Any(i => !i.IsExternalOnly));
        Assert.IsTrue(result.Items.Any(i => i.IsExternalOnly));
    }

    [Test]
    public async Task Filter_Paginates_Correctly_Across_Both_Stores()
    {
        // Arrange — create 3 members total (2 content, 1 external) so we can page through them.
        await CreateContentMemberAsync("a-content@test.com", "a-content");
        await CreateExternalMemberAsync("b-external@test.com", "b-external");
        await CreateContentMemberAsync("c-content@test.com", "c-content");

        // Act — get page 1 (take 2).
        PagedModel<MemberFilterItem> page1 = await MemberFilterService.FilterAsync(new MemberFilter(), orderBy: "username", skip: 0, take: 2);

        // Act — get page 2 (take 2).
        PagedModel<MemberFilterItem> page2 = await MemberFilterService.FilterAsync(new MemberFilter(), orderBy: "username", skip: 2, take: 2);

        // Assert
        Assert.AreEqual(3, page1.Total);
        Assert.AreEqual(2, page1.Items.Count());
        Assert.AreEqual(3, page2.Total);
        Assert.AreEqual(1, page2.Items.Count());

        // All 3 members should appear across both pages with no duplicates.
        var allUsernames = page1.Items.Concat(page2.Items).Select(i => i.UserName).ToList();
        Assert.AreEqual(3, allUsernames.Distinct().Count());
    }

    [Test]
    public async Task Filter_Orders_By_Username_Across_Both_Stores()
    {
        // Arrange — create members with known ordering.
        await CreateContentMemberAsync("z-content@test.com", "z-content");
        await CreateExternalMemberAsync("a-external@test.com", "a-external");
        await CreateContentMemberAsync("m-content@test.com", "m-content");

        // Act
        PagedModel<MemberFilterItem> result = await MemberFilterService.FilterAsync(
            new MemberFilter(), orderBy: "username", orderDirection: Direction.Ascending);

        // Assert — should be sorted: a-external, m-content, z-content.
        var usernames = result.Items.Select(i => i.UserName).ToList();
        Assert.AreEqual("a-external", usernames[0]);
        Assert.AreEqual("m-content", usernames[1]);
        Assert.AreEqual("z-content", usernames[2]);
    }

    [Test]
    public async Task Filter_By_MemberTypeId_Excludes_External_Members()
    {
        // Arrange
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        IMember contentMember = MemberBuilder.CreateSimpleMember(memberType, "content", "content@test.com", "password", "content-user");
        MemberService.Save(contentMember);

        await CreateExternalMemberAsync("external@test.com", "external-user");

        // Act — filter by the content member's type ID.
        PagedModel<MemberFilterItem> result = await MemberFilterService.FilterAsync(
            new MemberFilter { MemberTypeId = memberType.Key });

        // Assert — only the content member should be returned.
        Assert.AreEqual(1, result.Total);
        Assert.IsFalse(result.Items.First().IsExternalOnly);
    }

    [Test]
    public async Task Filter_By_IsApproved_Applies_To_Both_Stores()
    {
        // Arrange
        await CreateContentMemberAsync("approved-content@test.com", "approved-content");

        var unapprovedExternal = new ExternalMemberIdentityBuilder()
            .WithEmail("unapproved@test.com")
            .WithUserName("unapproved-external")
            .WithIsApproved(false)
            .Build();
        await ExternalMemberService.CreateAsync(unapprovedExternal);

        // Act — filter for approved only.
        PagedModel<MemberFilterItem> result = await MemberFilterService.FilterAsync(
            new MemberFilter { IsApproved = true });

        // Assert — only the approved content member.
        Assert.AreEqual(1, result.Total);
        Assert.AreEqual("approved-content", result.Items.First().UserName);
    }

    [Test]
    public async Task Filter_By_Text_Searches_Both_Stores()
    {
        // Arrange
        await CreateContentMemberAsync("alice-content@test.com", "alice-content");
        await CreateExternalMemberAsync("alice-external@test.com", "alice-external");
        await CreateContentMemberAsync("bob@test.com", "bob-content");

        // Act — search for "alice".
        PagedModel<MemberFilterItem> result = await MemberFilterService.FilterAsync(
            new MemberFilter { Filter = "alice" });

        // Assert — both Alice members, not Bob.
        Assert.AreEqual(2, result.Total);
        Assert.IsTrue(result.Items.All(i => i.UserName.Contains("alice")));
    }

    [Test]
    public async Task Filter_By_MemberTypeId_And_MemberGroupName()
    {
        // Arrange — create a content member in a specific type and group.
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        IMember contentMember = MemberBuilder.CreateSimpleMember(memberType, "grouped", "grouped@test.com", "password", "grouped-user");
        MemberService.Save(contentMember);

        MemberService.AddRole("FilterTestGroup");
        MemberService.AssignRoles([contentMember.Id], ["FilterTestGroup"]);

        // Create another content member of the same type but NOT in the group.
        IMember ungroupedMember = MemberBuilder.CreateSimpleMember(memberType, "ungrouped", "ungrouped@test.com", "password", "ungrouped-user");
        MemberService.Save(ungroupedMember);

        // Create an external member in the group (should be excluded by type filter).
        await CreateExternalMemberAsync("external@test.com", "external-user");
        await ExternalMemberService.AssignRolesAsync(
            (await ExternalMemberService.GetByUsernameAsync("external-user"))!.Key, ["FilterTestGroup"]);

        // Act — filter by both type and group.
        PagedModel<MemberFilterItem> result = await MemberFilterService.FilterAsync(
            new MemberFilter { MemberTypeId = memberType.Key, MemberGroupName = "FilterTestGroup" });

        // Assert — only the content member that matches both type AND group.
        Assert.AreEqual(1, result.Total);
        Assert.AreEqual("grouped-user", result.Items.First().UserName);
        Assert.IsFalse(result.Items.First().IsExternalOnly);
    }

    [Test]
    public async Task Filter_By_MemberGroupName_And_IsApproved()
    {
        // Arrange — create members in a group with different approval states.
        MemberService.AddRole("ApprovalTestGroup");

        await CreateContentMemberAsync("approved@test.com", "approved-content");
        IMember? approvedMember = MemberService.GetByEmail("approved@test.com");
        MemberService.AssignRoles([approvedMember!.Id], ["ApprovalTestGroup"]);

        var unapprovedExternal = new ExternalMemberIdentityBuilder()
            .WithEmail("unapproved@test.com")
            .WithUserName("unapproved-external")
            .WithIsApproved(false)
            .Build();
        var createResult = await ExternalMemberService.CreateAsync(unapprovedExternal);
        await ExternalMemberService.AssignRolesAsync(createResult.Result!.Key, ["ApprovalTestGroup"]);

        // Act — filter by group AND approved.
        PagedModel<MemberFilterItem> result = await MemberFilterService.FilterAsync(
            new MemberFilter { MemberGroupName = "ApprovalTestGroup", IsApproved = true });

        // Assert — only the approved content member.
        Assert.AreEqual(1, result.Total);
        Assert.AreEqual("approved-content", result.Items.First().UserName);
    }

    [Test]
    public async Task Filter_By_MemberGroupName_Returns_Both_Stores()
    {
        // Arrange — create members in the same group across both stores.
        MemberService.AddRole("SharedGroup");

        await CreateContentMemberAsync("content@test.com", "content-user");
        IMember? contentMember = MemberService.GetByEmail("content@test.com");
        MemberService.AssignRoles([contentMember!.Id], ["SharedGroup"]);

        await CreateExternalMemberAsync("external@test.com", "external-user");
        var externalMember = await ExternalMemberService.GetByUsernameAsync("external-user");
        await ExternalMemberService.AssignRolesAsync(externalMember!.Key, ["SharedGroup"]);

        // Act
        PagedModel<MemberFilterItem> result = await MemberFilterService.FilterAsync(
            new MemberFilter { MemberGroupName = "SharedGroup" });

        // Assert — both members from the shared group.
        Assert.AreEqual(2, result.Total);
        Assert.IsTrue(result.Items.Any(i => !i.IsExternalOnly));
        Assert.IsTrue(result.Items.Any(i => i.IsExternalOnly));
    }

    [Test]
    public async Task Filter_Empty_Returns_Empty()
    {
        // Act
        PagedModel<MemberFilterItem> result = await MemberFilterService.FilterAsync(new MemberFilter());

        // Assert
        Assert.AreEqual(0, result.Total);
        Assert.IsFalse(result.Items.Any());
    }

    private async Task CreateContentMemberAsync(string email, string username)
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        IMember member = MemberBuilder.CreateSimpleMember(memberType, username, email, "password123!", username);
        MemberService.Save(member);
    }

    private async Task CreateExternalMemberAsync(string email, string username)
    {
        var identity = new ExternalMemberIdentityBuilder()
            .WithEmail(email)
            .WithUserName(username)
            .WithName(username)
            .Build();
        await ExternalMemberService.CreateAsync(identity);
    }
}
