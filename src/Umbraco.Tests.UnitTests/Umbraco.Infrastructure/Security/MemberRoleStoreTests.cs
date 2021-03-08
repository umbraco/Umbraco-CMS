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

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security
{
    [TestFixture]
    public class MemberRoleStoreTests
    {
        private Mock<IMemberGroupService> _mockMemberGroupService;
        private IdentityErrorDescriber ErrorDescriber => new IdentityErrorDescriber();

        public MemberRoleStore<IdentityRole> CreateSut()
        {
            _mockMemberGroupService = new Mock<IMemberGroupService>();
            return new MemberRoleStore<IdentityRole>(
                _mockMemberGroupService.Object,
                ErrorDescriber);
        }

        [Test]
        public void GivenICreateAMemberRole_AndTheGroupIsNull_ThenIShouldGetAFailedIdentityResult()
        {
            // arrange
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeCancellationToken = new CancellationToken();

            // act
            Task<IdentityResult> actual = sut.CreateAsync(null, fakeCancellationToken);

            // assert
            Assert.IsFalse(actual.Result.Succeeded);
            Assert.IsTrue(actual.Result.Errors.Any(x => x.Code == "IdentityErrorUserStore" && x.Description == "Value cannot be null. (Parameter 'role')"));
        }


        [Test]
        public async Task GivenICreateAMemberRole_AndTheGroupIsPopulatedCorrectly_ThenIShouldGetASuccessResultAsync()
        {
            // arrange
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeRole = new IdentityRole
            {
                Id = "777",
                Name = "testname"
            };
            var fakeCancellationToken = new CancellationToken() { };

            IMemberGroup mockMemberGroup = Mock.Of<IMemberGroup>(m =>
                m.Name == "fakeGroupName" && m.CreatorId == 77);

            bool raiseEvents = false;

            _mockMemberGroupService.Setup(x => x.Save(mockMemberGroup, raiseEvents));

            // act
            IdentityResult identityResult = await sut.CreateAsync(fakeRole, fakeCancellationToken);

            // assert
            Assert.IsTrue(identityResult.Succeeded);
            Assert.IsTrue(!identityResult.Errors.Any());
            _mockMemberGroupService.Verify(x => x.Save(It.IsAny<MemberGroup>(), It.IsAny<bool>()));
        }

        [Test]
        public async Task GivenIUpdateAMemberRole_AndTheGroupExistsWithTheSameName_ThenIShouldGetASuccessResultAsyncButNoUpdatesMade()
        {
            // arrange
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeRole = new IdentityRole
            {
                Id = "777",
                Name = "fakeGroupName"
            };
            var fakeCancellationToken = new CancellationToken() { };

            IMemberGroup mockMemberGroup = Mock.Of<IMemberGroup>(m =>
                m.Name == "fakeGroupName" && m.CreatorId == 777);

            bool raiseEvents = false;

            _mockMemberGroupService.Setup(x => x.GetById(777)).Returns(mockMemberGroup);
            _mockMemberGroupService.Setup(x => x.Save(mockMemberGroup, raiseEvents));

            // act
            IdentityResult identityResult = await sut.UpdateAsync(fakeRole, fakeCancellationToken);

            // assert
            Assert.IsTrue(identityResult.Succeeded);
            Assert.IsTrue(!identityResult.Errors.Any());
            _mockMemberGroupService.Verify(x => x.GetById(777));
        }

        [Test]
        public async Task GivenIUpdateAMemberRole_AndTheGroupExistsWithADifferentSameName_ThenIShouldGetASuccessResultAsyncWithUpdatesMade()
        {
            // arrange
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeRole = new IdentityRole
            {
                Id = "777",
                Name = "fakeGroup777"
            };
            var fakeCancellationToken = new CancellationToken() { };

            IMemberGroup mockMemberGroup = Mock.Of<IMemberGroup>(m =>
                m.Name == "fakeGroupName" && m.CreatorId == 777);

            bool raiseEvents = false;

            _mockMemberGroupService.Setup(x => x.GetById(777)).Returns(mockMemberGroup);
            _mockMemberGroupService.Setup(x => x.Save(mockMemberGroup, raiseEvents));

            // act
            IdentityResult identityResult = await sut.UpdateAsync(fakeRole, fakeCancellationToken);

            // assert
            Assert.IsTrue(identityResult.Succeeded);
            Assert.IsTrue(!identityResult.Errors.Any());
            _mockMemberGroupService.Verify(x => x.Save(It.IsAny<IMemberGroup>(), It.IsAny<bool>()));
            _mockMemberGroupService.Verify(x => x.GetById(777));
        }

        [Test]
        public async Task GivenIUpdateAMemberRole_AndTheGroupDoesntExist_ThenIShouldGetAFailureResultAsync()
        {
            // arrange
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeRole = new IdentityRole
            {
                Id = "777",
                Name = "testname"
            };
            var fakeCancellationToken = new CancellationToken() { };

            // act
            IdentityResult identityResult = await sut.UpdateAsync(fakeRole, fakeCancellationToken);

            // assert
            Assert.IsTrue(identityResult.Succeeded == false);
            Assert.IsTrue(identityResult.Errors.Any(x => x.Code == "IdentityMemberGroupNotFound" && x.Description == "Member group not found"));
            _mockMemberGroupService.Verify(x => x.GetById(777));
        }

        [Test]
        public async Task GivenIUpdateAMemberRole_AndTheIdCannotBeParsedToAnInt_ThenIShouldGetAFailureResultAsync()
        {
            // arrange
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeRole = new IdentityRole
            {
                Id = "7a77",
                Name = "testname"
            };
            var fakeCancellationToken = new CancellationToken() { };

            // act
            IdentityResult identityResult = await sut.UpdateAsync(fakeRole, fakeCancellationToken);

            // assert
            Assert.IsTrue(identityResult.Succeeded == false);
            Assert.IsTrue(identityResult.Errors.Any(x => x.Code == "IdentityIdParseError" && x.Description == "Cannot parse ID to int"));
            _mockMemberGroupService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GivenIDeleteAMemberRole_AndItExists_ThenTheMemberGroupShouldBeDeleted_AndIShouldGetASuccessResultAsync()
        {
            // arrange
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeRole = new IdentityRole
            {
                Id = "777",
                Name = "testname"
            };
            var fakeCancellationToken = new CancellationToken() { };

            IMemberGroup mockMemberGroup = Mock.Of<IMemberGroup>(m =>
                m.Name == "fakeGroupName" && m.CreatorId == 77);

            _mockMemberGroupService.Setup(x => x.GetById(777)).Returns(mockMemberGroup);

            // act
            IdentityResult identityResult = await sut.DeleteAsync(fakeRole, fakeCancellationToken);

            // assert
            Assert.IsTrue(identityResult.Succeeded);
            Assert.IsTrue(!identityResult.Errors.Any());
            _mockMemberGroupService.Verify(x => x.GetById(777));
            _mockMemberGroupService.Verify(x => x.Delete(mockMemberGroup));
            _mockMemberGroupService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GivenIDeleteAMemberRole_AndTheIdCannotBeParsedToAnInt_ThenTheMemberGroupShouldNotBeDeleted_AndIShouldGetAFailResultAsync()
        {
            // arrange
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeRole = new IdentityRole
            {
                Id = "7a77",
                Name = "testname"
            };
            var fakeCancellationToken = new CancellationToken() { };

            IMemberGroup mockMemberGroup = Mock.Of<IMemberGroup>(m =>
                m.Name == "fakeGroupName" && m.CreatorId == 77);


            // act
            IdentityResult identityResult = await sut.DeleteAsync(fakeRole, fakeCancellationToken);

            // assert
            Assert.IsTrue(identityResult.Succeeded == false);
            Assert.IsTrue(identityResult.Errors.Any(x => x.Code == "IdentityIdParseError" && x.Description == "Cannot parse ID to int"));
            _mockMemberGroupService.VerifyNoOtherCalls();
        }


        [Test]
        public async Task GivenIDeleteAMemberRole_AndItDoesntExist_ThenTheMemberGroupShouldNotBeDeleted_AndIShouldGetAFailResultAsync()
        {
            // arrange
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeRole = new IdentityRole
            {
                Id = "777",
                Name = "testname"
            };
            var fakeCancellationToken = new CancellationToken() { };

            IMemberGroup mockMemberGroup = Mock.Of<IMemberGroup>(m =>
                m.Name == "fakeGroupName" && m.CreatorId == 77);


            // act
            IdentityResult identityResult = await sut.DeleteAsync(fakeRole, fakeCancellationToken);

            // assert
            Assert.IsTrue(identityResult.Succeeded == false);
            Assert.IsTrue(identityResult.Errors.Any(x => x.Code == "IdentityMemberGroupNotFound" && x.Description == "Member group not found"));
            _mockMemberGroupService.Verify(x => x.GetById(777));
            _mockMemberGroupService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GivenIFindAMemberRoleByRoleKey_AndRoleKeyExists_ThenIShouldGetASuccessResultAsync()
        {
            // arrange
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeRole = new IdentityRole("fakeGroupName")
            {
                Id = "777"
            };
            int fakeRoleId = 777;

            IMemberGroup fakeMemberGroup = new MemberGroup()
            {
                Name = "fakeGroupName",
                CreatorId = 123,
                Id = 777,
                Key = Guid.NewGuid()
            };

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
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeRole = new IdentityRole
            {
                Id = "7a77",
                Name = "testname"
            };
            var fakeCancellationToken = new CancellationToken() { };

            // act
            Action actual = () => sut.FindByIdAsync(fakeRole.Id, fakeCancellationToken);

            // assert
            Assert.That(actual, Throws.TypeOf<ArgumentOutOfRangeException>());
            _mockMemberGroupService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GivenIFindAMemberRoleByRoleId_AndIdCannotBeParsedToAnIntButCanBeToGuid_ThenIShouldGetASuccessResultAsync()
        {
            // arrange
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeRole = new IdentityRole("fakeGroupName")
            {
                Id = "777"
            };

            var fakeRoleGuid = Guid.NewGuid();

            IMemberGroup fakeMemberGroup = new MemberGroup()
            {
                Name = "fakeGroupName",
                CreatorId = 123,
                Id = 777,
                Key = fakeRoleGuid
            };

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
        public async Task GivenIFindAMemberRoleByRoleId_AndIdCannotBeParsedToAGuidButCanBeToInt_ThenIShouldGetASuccessResultAsync()
        {
            // arrange
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeRole = new IdentityRole("fakeGroupName")
            {
                Id = "777"
            };

            var fakeRoleId = 777;

            IMemberGroup fakeMemberGroup = new MemberGroup()
            {
                Name = "fakeGroupName",
                CreatorId = 123,
                Id = 777,
                Key = Guid.NewGuid()
            };

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
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeRole = new IdentityRole("fakeGroupName")
            {
                Id = "777"
            };

            IMemberGroup mockMemberGroup = Mock.Of<IMemberGroup>(m =>
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
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeRole = new IdentityRole
            {
                Id = "777"
            };
            var fakeCancellationToken = new CancellationToken() { };

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
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeCancellationToken = new CancellationToken() { };

            // act
            Action actual = () => sut.GetRoleIdAsync(null, fakeCancellationToken);

            // assert
            Assert.That(actual, Throws.ArgumentNullException);
        }

        [Test]
        public void GivenIGetAMemberRoleId_AndTheRoleIsNotNull_ThenIShouldGetTheMemberRole()
        {
            // arrange
            MemberRoleStore<IdentityRole> sut = CreateSut();
            var fakeRole = new IdentityRole("fakeGroupName")
            {
                Id = "777"
            };
            string fakeRoleId = fakeRole.Id;

            var fakeCancellationToken = new CancellationToken();

            // act
            Task<string> actual = sut.GetRoleIdAsync(fakeRole, fakeCancellationToken);

            // assert
            Assert.AreEqual(fakeRoleId, actual.Result);
        }
    }
}
