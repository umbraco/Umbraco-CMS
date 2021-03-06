using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security
{
    [TestFixture]
    public class MemberIdentityUserStoreTests
    {
        private Mock<IMemberService> _mockMemberService;

        public MemberUserStore CreateSut()
        {
            _mockMemberService = new Mock<IMemberService>();
            return new MemberUserStore(
                _mockMemberService.Object,
                new UmbracoMapper(new MapDefinitionCollection(new List<IMapDefinition>())),
                new Mock<IScopeProvider>().Object,
                new IdentityErrorDescriber());
        }

        [Test]
        public void GivenICreateUser_AndTheUserIsNull_ThenIShouldGetAFailedResultAsync()
        {
            // arrange
            MemberUserStore sut = CreateSut();
            CancellationToken fakeCancellationToken = new CancellationToken() { };

            // act
            Action actual = () => sut.CreateAsync(null, fakeCancellationToken);

            // assert
            Assert.That(actual, Throws.ArgumentNullException);
        }


        [Test]
        public async Task GivenICreateANewUser_AndTheUserIsPopulatedCorrectly_ThenIShouldGetASuccessResultAsync()
        {
            // arrange
            MemberUserStore sut = CreateSut();
            var fakeUser = new MemberIdentityUser() { };
            var fakeCancellationToken = new CancellationToken() { };

            IMemberType fakeMemberType = new MemberType(new MockShortStringHelper(), 77);
            IMember mockMember = Mock.Of<IMember>(m =>
                m.Name == "fakeName" &&
                m.Email == "fakeemail@umbraco.com" &&
                m.Username == "fakeUsername" &&
                m.RawPasswordValue == "fakePassword" &&
                m.ContentTypeAlias == fakeMemberType.Alias &&
                m.HasIdentity == true);

            bool raiseEvents = false;

            _mockMemberService.Setup(x => x.CreateMember(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(mockMember);
            _mockMemberService.Setup(x => x.Save(mockMember, raiseEvents));

            // act
            IdentityResult identityResult = await sut.CreateAsync(fakeUser, fakeCancellationToken);

            // assert
            Assert.IsTrue(identityResult.Succeeded);
            Assert.IsTrue(!identityResult.Errors.Any());
            _mockMemberService.Verify(x => x.CreateMember(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            _mockMemberService.Verify(x => x.Save(mockMember, It.IsAny<bool>()));
        }


        [Test]
        public async Task GivenIDeleteUser_AndTheUserIsNotPresent_ThenIShouldGetAFailedResultAsync()
        {
            // arrange
            MemberUserStore sut = CreateSut();
            var fakeUser = new MemberIdentityUser(777);
            var fakeCancellationToken = new CancellationToken() { };

            IMemberType fakeMemberType = new MemberType(new MockShortStringHelper(), 77);
            IMember mockMember = Mock.Of<IMember>(m =>
                m.Name == "fakeName" &&
                m.Email == "fakeemail@umbraco.com" &&
                m.Username == "fakeUsername" &&
                m.RawPasswordValue == "fakePassword" &&
                m.ContentTypeAlias == fakeMemberType.Alias &&
                m.HasIdentity == true);

            // act
            Action actual = () => sut.DeleteAsync(fakeUser, fakeCancellationToken);

            // assert
            Assert.That(actual, Throws.ArgumentNullException);
            _mockMemberService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GivenIDeleteUser_AndTheUserIsDeletedCorrectly_ThenIShouldGetASuccessResultAsync()
        {
            // arrange
            MemberUserStore sut = CreateSut();
            var fakeUser = new MemberIdentityUser(777);
            var fakeCancellationToken = new CancellationToken() { };

            IMemberType fakeMemberType = new MemberType(new MockShortStringHelper(), 77);
            IMember mockMember = new Member(fakeMemberType)
            {
                Id = 777,
                Name = "fakeName",
                Email = "fakeemail@umbraco.com",
                Username = "fakeUsername",
                RawPasswordValue = "fakePassword"
            };

            _mockMemberService.Setup(x => x.GetById(mockMember.Id)).Returns(mockMember);
            _mockMemberService.Setup(x => x.Delete(mockMember));

            // act
            IdentityResult identityResult = await sut.DeleteAsync(fakeUser, fakeCancellationToken);

            // assert
            Assert.IsTrue(identityResult.Succeeded);
            Assert.IsTrue(!identityResult.Errors.Any());
            _mockMemberService.Verify(x => x.GetById(mockMember.Id));
            _mockMemberService.Verify(x => x.Delete(mockMember));
            _mockMemberService.VerifyNoOtherCalls();
        }
    }
}
