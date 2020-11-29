using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Members;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Infrastructure.Members;
using Umbraco.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.Members
{
    [TestFixture]
    public class UmbracoMemberIdentityUserStoreTests
    {
        private Mock<IMemberService> _mockMemberService;

        public UmbracoMembersUserStore CreateSut()
        {
            _mockMemberService = new Mock<IMemberService>();
            return new UmbracoMembersUserStore(_mockMemberService.Object);
        }

        [Test]
        public void GivenICreateUser_AndTheUserIsNull_ThenIShouldGetAFailedResultAsync()
        {
            //arrange
            UmbracoMembersUserStore sut = CreateSut();
            CancellationToken fakeCancellationToken = new CancellationToken(){};

            //act
            Action actual = () => sut.CreateAsync(null, fakeCancellationToken);

            //assert
            Assert.That(actual, Throws.ArgumentNullException);
        }


        [Test]
        public async Task GivenICreateUser_AndTheUserIsPopulatedCorrectly_ThenIShouldGetASuccessResult()
        {
            //arrange
            UmbracoMembersUserStore sut = CreateSut();
            UmbracoMembersIdentityUser fakeUser = new UmbracoMembersIdentityUser()
            {
                
            };

            CancellationToken fakeCancellationToken = new CancellationToken() { };

            IMemberType fakeMemberType = new MemberType(new MockShortStringHelper(), 77);

            var mockMember = Mock.Of<IMember>(m =>
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
        }
    }
}
