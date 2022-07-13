using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security;

[TestFixture]
public class MemberRoleStoreTests
{
    [SetUp]
    public void SetUp()
    {
        _roleBuilder = new UmbracoIdentityRoleBuilder();
        _groupBuilder = new MemberGroupBuilder();
    }

    private Mock<IMemberGroupService> _mockMemberGroupService;

    private IdentityErrorDescriber ErrorDescriber => new();

    private UmbracoIdentityRoleBuilder _roleBuilder;

    private MemberGroupBuilder _groupBuilder;

    public MemberRoleStore CreateSut()
    {
        _mockMemberGroupService = new Mock<IMemberGroupService>();
        return new MemberRoleStore(
            _mockMemberGroupService.Object,
            ErrorDescriber);
    }

    [Test]
    public void GivenICreateAMemberRole_AndTheGroupIsNull_ThenIShouldGetAFailedIdentityResult()
    {
        // arrange
        var sut = CreateSut();
        var fakeCancellationToken = CancellationToken.None;

        // act
        Action actual = () => sut.CreateAsync(null, fakeCancellationToken);

        // assert
        Assert.That(actual, Throws.ArgumentNullException);
        _mockMemberGroupService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenICreateAMemberRole_AndTheGroupIsPopulatedCorrectly_ThenIShouldGetASuccessResultAsync()
    {
        // arrange
        var sut = CreateSut();
        var fakeRole = _roleBuilder.WithTestName("777").Build();
        var fakeCancellationToken = CancellationToken.None;

        var mockMemberGroup = Mock.Of<IMemberGroup>(m =>
            m.Name == "fakeGroupName" && m.CreatorId == 77);

        _mockMemberGroupService.Setup(x => x.Save(mockMemberGroup));

        // act
        var identityResult = await sut.CreateAsync(fakeRole, fakeCancellationToken);

        // assert
        Assert.IsTrue(identityResult.Succeeded);
        Assert.IsTrue(!identityResult.Errors.Any());
        _mockMemberGroupService.Verify(x => x.Save(It.IsAny<MemberGroup>()));
    }

    [Test]
    public async Task
        GivenIUpdateAMemberRole_AndTheGroupExistsWithTheSameName_ThenIShouldGetASuccessResultAsyncButNoUpdatesMade()
    {
        // arrange
        var sut = CreateSut();
        var fakeRole = _roleBuilder.WithName("fakeGroupName").WithId("777").Build();
        var fakeCancellationToken = CancellationToken.None;

        var mockMemberGroup = Mock.Of<IMemberGroup>(m =>
            m.Name == "fakeGroupName" && m.CreatorId == 777);

        _mockMemberGroupService.Setup(x => x.GetById(777)).Returns(mockMemberGroup);
        _mockMemberGroupService.Setup(x => x.Save(mockMemberGroup));

        // act
        var identityResult = await sut.UpdateAsync(fakeRole, fakeCancellationToken);

        // assert
        Assert.IsTrue(identityResult.Succeeded);
        Assert.IsTrue(!identityResult.Errors.Any());
        _mockMemberGroupService.Verify(x => x.GetById(777));
    }

    [Test]
    public async Task
        GivenIUpdateAMemberRole_AndTheGroupExistsWithADifferentSameName_ThenIShouldGetASuccessResultAsyncWithUpdatesMade()
    {
        // arrange
        var sut = CreateSut();
        var fakeRole = _roleBuilder.WithName("fakeGroup777").WithId("777").Build();
        var fakeCancellationToken = CancellationToken.None;

        var mockMemberGroup = Mock.Of<IMemberGroup>(m =>
            m.Name == "fakeGroupName" && m.CreatorId == 777);

        _mockMemberGroupService.Setup(x => x.GetById(777)).Returns(mockMemberGroup);
        _mockMemberGroupService.Setup(x => x.Save(mockMemberGroup));

        // act
        var identityResult = await sut.UpdateAsync(fakeRole, fakeCancellationToken);

        // assert
        Assert.IsTrue(identityResult.Succeeded);
        Assert.IsTrue(!identityResult.Errors.Any());
        _mockMemberGroupService.Verify(x => x.Save(It.IsAny<IMemberGroup>()));
        _mockMemberGroupService.Verify(x => x.GetById(777));
    }

    [Test]
    public async Task GivenIUpdateAMemberRole_AndTheGroupDoesntExist_ThenIShouldGetAFailureResultAsync()
    {
        // arrange
        var sut = CreateSut();
        var fakeRole = _roleBuilder.WithTestName("777").Build();
        var fakeCancellationToken = CancellationToken.None;

        // act
        var identityResult = await sut.UpdateAsync(fakeRole, fakeCancellationToken);

        // assert
        Assert.IsTrue(identityResult.Succeeded == false);
        Assert.IsTrue(identityResult.Errors.Any(x =>
            x.Code == "IdentityMemberGroupNotFound" && x.Description == "Member group not found"));
        _mockMemberGroupService.Verify(x => x.GetById(777));
    }

    [Test]
    public async Task GivenIUpdateAMemberRole_AndTheIdCannotBeParsedToAnInt_ThenIShouldGetAFailureResultAsync()
    {
        // arrange
        var sut = CreateSut();
        var fakeRole = _roleBuilder.WithTestName("7a77").Build();
        var fakeCancellationToken = CancellationToken.None;

        // act
        var identityResult = await sut.UpdateAsync(fakeRole, fakeCancellationToken);

        // assert
        Assert.IsTrue(identityResult.Succeeded == false);
        Assert.IsTrue(identityResult.Errors.Any(x =>
            x.Code == "IdentityIdParseError" && x.Description == "Cannot parse ID to int"));
        _mockMemberGroupService.VerifyNoOtherCalls();
    }

    [Test]
    public void GivenIUpdateAMemberRole_AndTheRoleIsNull_ThenAnExceptionShouldBeThrown()
    {
        // arrange
        var sut = CreateSut();
        var fakeCancellationToken = CancellationToken.None;

        // act
        Action actual = () => sut.UpdateAsync(null, fakeCancellationToken);

        // assert
        Assert.That(actual, Throws.ArgumentNullException);
        _mockMemberGroupService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task
        GivenIDeleteAMemberRole_AndItExists_ThenTheMemberGroupShouldBeDeleted_AndIShouldGetASuccessResultAsync()
    {
        // arrange
        var sut = CreateSut();
        var fakeRole = _roleBuilder.WithTestName("777").Build();
        var fakeCancellationToken = CancellationToken.None;

        var mockMemberGroup = Mock.Of<IMemberGroup>(m =>
            m.Name == "fakeGroupName" && m.CreatorId == 77);

        _mockMemberGroupService.Setup(x => x.GetById(777)).Returns(mockMemberGroup);

        // act
        var identityResult = await sut.DeleteAsync(fakeRole, fakeCancellationToken);

        // assert
        Assert.IsTrue(identityResult.Succeeded);
        Assert.IsTrue(!identityResult.Errors.Any());
        _mockMemberGroupService.Verify(x => x.GetById(777));
        _mockMemberGroupService.Verify(x => x.Delete(mockMemberGroup));
        _mockMemberGroupService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task
        GivenIDeleteAMemberRole_AndTheIdCannotBeParsedToAnInt_ThenTheMemberGroupShouldNotBeDeleted_AndIShouldGetAnArgumentException()
    {
        // arrange
        var sut = CreateSut();
        var fakeRole = _roleBuilder.WithTestName("7a77").Build();
        var fakeCancellationToken = CancellationToken.None;

        var mockMemberGroup = Mock.Of<IMemberGroup>(m =>
            m.Name == "fakeGroupName" && m.CreatorId == 77);

        // act
        Assert.ThrowsAsync<ArgumentException>(async () => await sut.DeleteAsync(fakeRole, fakeCancellationToken));
    }

    [Test]
    public async Task
        GivenIDeleteAMemberRole_AndItDoesntExist_ThenTheMemberGroupShouldNotBeDeleted_AndIShouldGetAFailResultAsync()
    {
        // arrange
        var sut = CreateSut();
        var fakeRole = _roleBuilder.WithTestName("777").Build();
        var fakeCancellationToken = CancellationToken.None;

        var mockMemberGroup = Mock.Of<IMemberGroup>(m =>
            m.Name == "fakeGroupName" && m.CreatorId == 77);

        // act
        var identityResult = await sut.DeleteAsync(fakeRole, fakeCancellationToken);

        // assert
        Assert.IsTrue(identityResult.Succeeded == false);
        Assert.IsTrue(identityResult.Errors.Any(x =>
            x.Code == "IdentityMemberGroupNotFound" && x.Description == "Member group not found"));
        _mockMemberGroupService.Verify(x => x.GetById(777));
        _mockMemberGroupService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenIFindAMemberRoleByRoleKey_AndRoleKeyExists_ThenIShouldGetASuccessResultAsync()
    {
        // arrange
        var sut = CreateSut();
        var fakeRole = _roleBuilder.WithName("fakeGroupName").WithId("777").Build();
        var fakeRoleId = 777;

        IMemberGroup fakeMemberGroup = _groupBuilder.WithName("fakeGroupName").WithCreatorId(123).WithId(777)
            .WithKey(Guid.NewGuid()).Build();

        _mockMemberGroupService.Setup(x => x.GetById(fakeRoleId)).Returns(fakeMemberGroup);

        // act
        IdentityRole actual = await sut.FindByIdAsync(fakeRole.Id);

        // assert
        Assert.AreEqual(fakeRole.Name, actual.Name);
        Assert.AreEqual(fakeRole.Id, actual.Id);
        _mockMemberGroupService.Verify(x => x.GetById(fakeRoleId));
        _mockMemberGroupService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenIFindAMemberRoleByRoleId_AndIdCannotBeParsedToAnIntOrGuid_ThenIShouldGetAFailureResultAsync()
    {
        // arrange
        var sut = CreateSut();
        var fakeRole = _roleBuilder.WithTestName("7a77").Build();
        var fakeCancellationToken = CancellationToken.None;

        // act
        Action actual = () => sut.FindByIdAsync(fakeRole.Id, fakeCancellationToken);

        // assert
        Assert.That(actual, Throws.TypeOf<ArgumentOutOfRangeException>());
        _mockMemberGroupService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task
        GivenIFindAMemberRoleByRoleId_AndIdCannotBeParsedToAnIntButCanBeToGuid_ThenIShouldGetASuccessResultAsync()
    {
        // arrange
        var sut = CreateSut();
        var fakeRole = _roleBuilder.WithName("fakeGroupName").WithId("777").Build();

        var fakeRoleGuid = Guid.NewGuid();

        IMemberGroup fakeMemberGroup = _groupBuilder.WithName("fakeGroupName").WithCreatorId(123).WithId(777)
            .WithKey(fakeRoleGuid).Build();

        _mockMemberGroupService.Setup(x => x.GetById(fakeRoleGuid)).Returns(fakeMemberGroup);

        // act
        IdentityRole actual = await sut.FindByIdAsync(fakeRoleGuid.ToString());

        // assert
        Assert.AreEqual(fakeRole.Name, actual.Name);
        Assert.AreEqual(fakeRole.Id, actual.Id);
        _mockMemberGroupService.Verify(x => x.GetById(fakeRoleGuid));
        _mockMemberGroupService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task
        GivenIFindAMemberRoleByRoleId_AndIdCannotBeParsedToAGuidButCanBeToInt_ThenIShouldGetASuccessResultAsync()
    {
        // arrange
        var sut = CreateSut();
        var fakeRole = _roleBuilder.WithName("fakeGroupName").WithId("777").Build();

        var fakeRoleId = 777;

        IMemberGroup fakeMemberGroup = _groupBuilder.WithName("fakeGroupName").WithCreatorId(123).WithId(777)
            .WithKey(Guid.NewGuid()).Build();

        _mockMemberGroupService.Setup(x => x.GetById(fakeRoleId)).Returns(fakeMemberGroup);

        // act
        IdentityRole actual = await sut.FindByIdAsync(fakeRoleId.ToString());

        // assert
        Assert.AreEqual(fakeRole.Name, actual.Name);
        Assert.AreEqual(fakeRole.Id, actual.Id);
        _mockMemberGroupService.Verify(x => x.GetById(fakeRoleId));
        _mockMemberGroupService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenIFindAMemberRoleByRoleName_AndRoleNameExists_ThenIShouldGetASuccessResultAsync()
    {
        // arrange
        var sut = CreateSut();
        var fakeRole = _roleBuilder.WithName("fakeGroupName").WithId("777").Build();

        var mockMemberGroup = Mock.Of<IMemberGroup>(m =>
            m.Name == "fakeGroupName" &&
            m.CreatorId == 123 &&
            m.Id == 777);

        _mockMemberGroupService.Setup(x => x.GetByName(fakeRole.Name)).Returns(mockMemberGroup);

        // act
        IdentityRole actual = await sut.FindByNameAsync(fakeRole.Name);

        // assert
        Assert.AreEqual(fakeRole.Name, actual.Name);
        Assert.AreEqual(fakeRole.Id, actual.Id);
        _mockMemberGroupService.Verify(x => x.GetByName(fakeRole.Name));
    }

    [Test]
    public void GivenIFindAMemberRoleByRoleName_AndTheNameIsNull_ThenIShouldGetAnArgumentException()
    {
        // arrange
        var sut = CreateSut();
        var fakeRole = _roleBuilder.WithId("777").Build();
        var fakeCancellationToken = CancellationToken.None;

        // act
        Action actual = () => sut.FindByNameAsync(fakeRole.Name, fakeCancellationToken);

        // assert
        Assert.That(actual, Throws.ArgumentNullException);
        _mockMemberGroupService.VerifyNoOtherCalls();
    }

    [Test]
    public void GivenIGetAMemberRoleId_AndTheRoleIsNull_ThenIShouldGetAnArgumentException()
    {
        // arrange
        var sut = CreateSut();
        var fakeCancellationToken = CancellationToken.None;

        // act
        Action actual = () => sut.GetRoleIdAsync(null, fakeCancellationToken);

        // assert
        Assert.That(actual, Throws.ArgumentNullException);
    }

    [Test]
    public void GivenIGetAMemberRoleId_AndTheRoleIsNotNull_ThenIShouldGetTheMemberRole()
    {
        // arrange
        var sut = CreateSut();
        var fakeRole = _roleBuilder.WithName("fakeGroupName").WithId("777").Build();
        var fakeRoleId = fakeRole.Id;

        var fakeCancellationToken = CancellationToken.None;

        // act
        var actual = sut.GetRoleIdAsync(fakeRole, fakeCancellationToken);

        // assert
        Assert.AreEqual(fakeRoleId, actual.Result);
    }
}
