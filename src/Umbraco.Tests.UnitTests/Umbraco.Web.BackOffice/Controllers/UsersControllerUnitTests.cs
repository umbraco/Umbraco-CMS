using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using Umbraco.Core.BackOffice;
using Umbraco.Tests.Common.AutoFixture;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Common.Exceptions;

namespace Umbraco.Tests.Web.Controllers
{
    [TestFixture]
    public class UsersControllerUnitTests
    {
        [Test,AutoMoqData]
        public async Task PostUnlockUsers_When_User_Lockout_Update_Fails_Expect_Failure_Response(
            [Frozen] IUserStore<BackOfficeIdentityUser> userStore,
            UsersController sut,
            BackOfficeIdentityUser user,
            int[] userIds,
            string expectedMessage)
        {
            Mock.Get(userStore)
                .Setup(x => x.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            Assert.ThrowsAsync<HttpResponseException>(() => sut.PostUnlockUsers(userIds));
        }

    }
}
