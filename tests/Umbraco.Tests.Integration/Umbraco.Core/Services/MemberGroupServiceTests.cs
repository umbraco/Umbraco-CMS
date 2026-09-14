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
internal sealed class MemberGroupServiceTests : UmbracoIntegrationTest
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
            Assert.That(fetchedGroup, Is.Not.Null);
            Assert.That(memberGroup, Is.EqualTo(fetchedGroup));
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
            Assert.That(fetchedGroup, Is.Not.Null);
            Assert.That(fetchedGroup.Key, Is.EqualTo(key));
            Assert.That(memberGroup, Is.EqualTo(fetchedGroup));
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
            Assert.That(fetchedGroup, Is.Not.Null);
            Assert.That(fetchedGroup.Name, Is.EqualTo(updatedName));
            Assert.That(memberGroup, Is.EqualTo(fetchedGroup));
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

        Assert.That(fetchedGroup, Is.Not.Null);

        await MemberGroupService.DeleteAsync(memberGroup.Key);

        // re-get
        fetchedGroup = await MemberGroupService.GetAsync(memberGroup.Key);

        Assert.That(fetchedGroup, Is.Null);
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
            Assert.That(attemptOne.Success, Is.True);
            Assert.That(attemptOne.Status, Is.EqualTo(MemberGroupOperationStatus.Success));
        });

        var attemptTwo = await MemberGroupService.CreateAsync(memberGroupTwo);

        Assert.Multiple(() =>
        {
            Assert.That(attemptTwo.Success, Is.False);
            Assert.That(attemptTwo.Status, Is.EqualTo(MemberGroupOperationStatus.DuplicateName));
        });
    }

    [Test]
    public async Task Can_Get_MemberGroups_By_Keys()
    {
        var groupOne = new MemberGroup { Name = "GroupOne" };
        var groupTwo = new MemberGroup { Name = "GroupTwo" };
        var groupThree = new MemberGroup { Name = "GroupThree" };
        await MemberGroupService.CreateAsync(groupOne);
        await MemberGroupService.CreateAsync(groupTwo);
        await MemberGroupService.CreateAsync(groupThree);

        IMemberGroup[] result = (await MemberGroupService.GetAsync([groupOne.Key, groupThree.Key])).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result.Any(x => x.Key == groupOne.Key), Is.True);
            Assert.That(result.Any(x => x.Key == groupThree.Key), Is.True);
            Assert.That(result.Any(x => x.Key == groupTwo.Key), Is.False);
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
            Assert.That(attemptOne.Success, Is.True);
            Assert.That(attemptOne.Status, Is.EqualTo(MemberGroupOperationStatus.Success));
            Assert.That(attemptTwo.Success, Is.True);
            Assert.That(attemptTwo.Status, Is.EqualTo(MemberGroupOperationStatus.Success));
        });

        // Update to already existing name.
        memberGroupTwo.Name = name;
        var updateAttempt = await MemberGroupService.UpdateAsync(memberGroupTwo);

        Assert.Multiple(() =>
        {
            Assert.That(updateAttempt.Success, Is.False);
            Assert.That(updateAttempt.Status, Is.EqualTo(MemberGroupOperationStatus.DuplicateName));
        });
    }
}
