using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security
{
    [TestFixture]
    public class MemberRoleStoreTests
    {
        private Mock<IMemberGroupService> _mockMemberGroupService;
        private IdentityErrorDescriber ErrorDescriber => new IdentityErrorDescriber();

        public MemberRoleStore CreateSut()
        {
            _mockMemberGroupService = new Mock<IMemberGroupService>();
            return new MemberRoleStore(
                _mockMemberGroupService.Object,
                ErrorDescriber);
        }

        [Test]
        public void GivenICreateAMemberRole_AndTheGroupIsNull_ThenIShouldGetAFailedResultAsync()
        {
            // arrange
            MemberRoleStore sut = CreateSut();
            CancellationToken fakeCancellationToken = new CancellationToken() { };

            // act
            Action actual = () => sut.CreateAsync(null, fakeCancellationToken);

            // assert
            Assert.That(actual, Throws.ArgumentNullException);
        }


        [Test]
        public async Task GivenICreateAMemberRole_AndTheGroupIsPopulatedCorrectly_ThenIShouldGetASuccessResultAsync()
        {
            // arrange
            MemberRoleStore sut = CreateSut();
            var fakeRole = new IdentityRole<string>()
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
            MemberRoleStore sut = CreateSut();
            var fakeRole = new IdentityRole<string>()
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
            MemberRoleStore sut = CreateSut();
            var fakeRole = new IdentityRole<string>()
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
            MemberRoleStore sut = CreateSut();
            var fakeRole = new IdentityRole<string>()
            {
                Id = "777",
                Name = "testname"
            };
            var fakeCancellationToken = new CancellationToken() { };

            bool raiseEvents = false;


            // act
            IdentityResult identityResult = await sut.UpdateAsync(fakeRole, fakeCancellationToken);

            // assert
            Assert.IsTrue(identityResult.Succeeded == false);
            Assert.IsTrue(identityResult.Errors.Any(x => x.Code == "InvalidRoleName" && x.Description == "Role name 'testname' is invalid."));
            _mockMemberGroupService.Verify(x => x.GetById(777));
        }

        [Test]
        public async Task GivenIUpdateAMemberRole_AndTheIdCannotBeParsedToAnInt_ThenIShouldGetAFailureResultAsync()
        {
            // arrange
            MemberRoleStore sut = CreateSut();
            var fakeRole = new IdentityRole<string>()
            {
                Id = "7a77",
                Name = "testname"
            };
            var fakeCancellationToken = new CancellationToken() { };

            bool raiseEvents = false;


            // act
            IdentityResult identityResult = await sut.UpdateAsync(fakeRole, fakeCancellationToken);

            // assert
            Assert.IsTrue(identityResult.Succeeded == false);
            Assert.IsTrue(identityResult.Errors.Any(x => x.Code == "DefaultError" && x.Description == "An unknown failure has occurred."));
        }

        [Test]
        public async Task GivenIDeleteAMemberRole_AndItExists_ThenTheMemberGroupShouldBeDeleted_AndIShouldGetASuccessResultAsync()
        {
            // arrange
            MemberRoleStore sut = CreateSut();
            var fakeRole = new IdentityRole<string>()
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
        }

        [Test]
        public async Task GivenIDeleteAMemberRole_AndTheIdCannotBeParsedToAnInt_ThenTheMemberGroupShouldNotBeDeleted_AndIShouldGetAFailResultAsync()
        {
            // arrange
            MemberRoleStore sut = CreateSut();
            var fakeRole = new IdentityRole<string>()
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
            Assert.IsTrue(identityResult.Errors.Any(x => x.Code == "DefaultError" && x.Description == "An unknown failure has occurred."));
        }


        [Test]
        public async Task GivenIDeleteAMemberRole_AndItDoesntExist_ThenTheMemberGroupShouldNotBeDeleted_AndIShouldGetAFailResultAsync()
        {
            // arrange
            MemberRoleStore sut = CreateSut();
            var fakeRole = new IdentityRole<string>()
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
            Assert.IsTrue(identityResult.Errors.Any(x=>x.Code == "InvalidRoleName" && x.Description == "Role name 'testname' is invalid."));
            _mockMemberGroupService.Verify(x => x.GetById(777));
        }

        [Test]
        public async Task GivenIGetAllMemberRoles_ThenIShouldGetAllMemberGroups_AndASuccessResultAsync()
        {
            // arrange
            MemberRoleStore sut = CreateSut();
            var fakeRole = new IdentityRole<string>("fakeGroupName")
            {
                Id = "777"
            };
            IEnumerable<IdentityRole<string>> expected = new List<IdentityRole<string>>()
            {
                fakeRole
            };

            IMemberGroup mockMemberGroup = Mock.Of<IMemberGroup>(m =>
                m.Name == "fakeGroupName" && m.CreatorId == 123 && m.Id == 777);

            IEnumerable<IMemberGroup> fakeMemberGroups = new List<IMemberGroup>()
            {
                mockMemberGroup
            };

            _mockMemberGroupService.Setup(x => x.GetAll()).Returns(fakeMemberGroups);

            // act
            IQueryable<IdentityRole<string>> actual = sut.Roles;

            // assert
            Assert.AreEqual(expected.AsQueryable().First().Id, actual.First().Id);
            Assert.AreEqual(expected.AsQueryable().First().Name, actual.First().Name);
            //Always null:
            //Assert.AreEqual(expected.AsQueryable().First().NormalizedName, actual.First().NormalizedName);
            //Always different:
            //Assert.AreEqual(expected.AsQueryable().First().ConcurrencyStamp, actual.First().ConcurrencyStamp);
            _mockMemberGroupService.Verify(x => x.GetAll());
        }
    }
}
