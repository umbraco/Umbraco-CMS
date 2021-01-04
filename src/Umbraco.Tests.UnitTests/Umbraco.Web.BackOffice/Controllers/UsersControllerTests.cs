// Copyright (c) Umbraco.
// See LICENSE for more details.

using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Security;
using Umbraco.Tests.UnitTests.AutoFixture;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Common.Exceptions;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.BackOffice.Controllers
{
    [TestFixture]
    public class UsersControllerTests
    {
        [Test]
        [AutoMoqData]
        public void PostUnlockUsers_When_User_Lockout_Update_Fails_Expect_Failure_Response(
            [Frozen] IBackOfficeUserManager backOfficeUserManager,
            UsersController sut,
            BackOfficeIdentityUser user,
            int[] userIds,
            string expectedMessage)
        {
            Mock.Get(backOfficeUserManager)
                .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            Assert.ThrowsAsync<HttpResponseException>(() => sut.PostUnlockUsers(userIds));
        }
    }
}
