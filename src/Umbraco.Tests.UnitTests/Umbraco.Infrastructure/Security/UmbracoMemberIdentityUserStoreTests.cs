using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Infrastructure.Security;
using Umbraco.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.Members
{
    [TestFixture]
    public class UmbracoMemberIdentityUserStoreTests
    {
        private Mock<IMemberService> _mockMemberService;

        public MembersUserStore CreateSut()
        {
            _mockMemberService = new Mock<IMemberService>();
            return new MembersUserStore(
                _mockMemberService.Object,
                new UmbracoMapper(new MapDefinitionCollection(new List<IMapDefinition>())),
                new Mock<IScopeProvider>().Object,
                new IdentityErrorDescriber());
        }

        [Test]
        public void GivenICreateUser_AndTheUserIsNull_ThenIShouldGetAFailedResultAsync()
        {
            //arrange
            MembersUserStore sut = CreateSut();
            CancellationToken fakeCancellationToken = new CancellationToken(){};

            //act
            Action actual = () => sut.CreateAsync(null, fakeCancellationToken);

            //assert
            Assert.That(actual, Throws.ArgumentNullException);
        }


        [Test]
        public async Task GivenICreateANewUser_AndTheUserIsPopulatedCorrectly_ThenIShouldGetASuccessResultAsync()
        {
            //arrange
            MembersUserStore sut = CreateSut();
            MembersIdentityUser fakeUser = new MembersIdentityUser() { };
            CancellationToken fakeCancellationToken = new CancellationToken() { };

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

            //act
            IdentityResult identityResult = await sut.CreateAsync(fakeUser, fakeCancellationToken);

            //assert
            Assert.IsTrue(identityResult.Succeeded);
            Assert.IsTrue(!identityResult.Errors.Any());
        }

        //FindByNameAsync
        [Test]
        public async Task GivenIGetUserNameAsync()
        {
        }

        [Test]
        public async Task GivenIFindByNameAsync()
        {
        }

        //SetNormalizedUserNameAsync
        //SetUserNameAsync
        //HasPasswordAsync
        //GetPasswordHashAsync
        //SetPasswordHashAsync
        //GetUserIdAsync
    }
}
