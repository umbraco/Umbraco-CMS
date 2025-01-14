using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Tests covering the MemberGroupService
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class MemberGroupServiceTests : UmbracoIntegrationTest
{
    private IMemberGroupService MemberGroupService => GetRequiredService<IMemberGroupService>();

    [Test]
    public async Task Can_Create_MemberGroup()
    {
        var memberGroup = new MemberGroup
        {
            Name = "TestGroup",
        };
        await MemberGroupService.CreateAsync(memberGroup);

        var fetchedGroup = await MemberGroupService.GetAsync(memberGroup.Key);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(fetchedGroup);
            Assert.AreEqual(fetchedGroup, memberGroup);
        });
    }

    [Test]
    public async Task Can_Create_MemberGroup_With_Key()
    {
        Guid key = Guid.NewGuid();
        var memberGroup = new MemberGroup
        {
            Name = "TestGroup",
            Key = key,
        };
        await MemberGroupService.CreateAsync(memberGroup);

        var fetchedGroup = await MemberGroupService.GetAsync(memberGroup.Key);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(fetchedGroup);
            Assert.AreEqual(key, fetchedGroup.Key);
            Assert.AreEqual(fetchedGroup, memberGroup);
        });
    }

    [Test]
    public async Task Can_Update_MemberGroup()
    {
        const string updatedName = "UpdatedName";
        var memberGroup = new MemberGroup
        {
            Name = "TestGroup",
        };
        await MemberGroupService.CreateAsync(memberGroup);

        memberGroup.Name = updatedName;
        await MemberGroupService.UpdateAsync(memberGroup);

        var fetchedGroup = await MemberGroupService.GetAsync(memberGroup.Key);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(fetchedGroup);
            Assert.AreEqual(fetchedGroup.Name, updatedName);
            Assert.AreEqual(fetchedGroup, memberGroup);
        });
    }

    [Test]
    public async Task Can_Delete_MemberGroup()
    {
        var memberGroup = new MemberGroup
        {
            Name = "TestGroup",
        };
        await MemberGroupService.CreateAsync(memberGroup);

        var fetchedGroup = await MemberGroupService.GetAsync(memberGroup.Key);

        Assert.IsNotNull(fetchedGroup);

        await MemberGroupService.DeleteAsync(memberGroup.Key);

        // re-get
        fetchedGroup = await MemberGroupService.GetAsync(memberGroup.Key);

        Assert.IsNull(fetchedGroup);
    }

    [Test]
    public async Task Cannot_Create_MemberGroup_With_Duplicate_Name()
    {
        const string name = "TestGroup";
        var memberGroupOne = new MemberGroup
        {
            Name = name,
        };
        var memberGroupTwo = new MemberGroup
        {
            Name = name,
        };
        var attemptOne = await MemberGroupService.CreateAsync(memberGroupOne);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(attemptOne.Success);
            Assert.AreEqual(MemberGroupOperationStatus.Success, attemptOne.Status);
        });

        var attemptTwo = await MemberGroupService.CreateAsync(memberGroupTwo);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(attemptTwo.Success);
            Assert.AreEqual(MemberGroupOperationStatus.DuplicateName, attemptTwo.Status);
        });
    }

    [Test]
    public async Task Cannot_Update_MemberGroup_With_Duplicate_Name()
    {
        const string name = "TestGroup";
        var memberGroupOne = new MemberGroup
        {
            Name = name,
        };
        var memberGroupTwo = new MemberGroup
        {
            Name = "TestGroupTwo",
        };
        var attemptOne = await MemberGroupService.CreateAsync(memberGroupOne);
        var attemptTwo = await MemberGroupService.CreateAsync(memberGroupTwo);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(attemptOne.Success);
            Assert.AreEqual(MemberGroupOperationStatus.Success, attemptOne.Status);
            Assert.IsTrue(attemptTwo.Success);
            Assert.AreEqual(MemberGroupOperationStatus.Success, attemptTwo.Status);
        });

        // Update to already existing name.
        memberGroupTwo.Name = name;
        var updateAttempt = await MemberGroupService.UpdateAsync(memberGroupTwo);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(updateAttempt.Success);
            Assert.AreEqual(MemberGroupOperationStatus.DuplicateName, updateAttempt.Status);
        });
    }
}
