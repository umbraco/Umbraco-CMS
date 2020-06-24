using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Models.Membership;
using Umbraco.Tests.Common.AutoFixture;
using Umbraco.Web.BackOffice.Controllers;

namespace Umbraco.Tests.Web.Controllers
{
    [TestFixture]
    public class UsersControllerUnitTests
    {
        [Test]
        [AutoMoqData]
        public void PostUnlockUsers_When_User_Lockout_Update_Fails_Expect_Failure_Response(
            [Frozen(Matching.ParameterName)] Mock<BackOfficeUserManager> backOfficeUserManager,
            UsersController sut,
            BackOfficeIdentityUser[] users,
            int[] userIds,
            string expectedMessage)
        {
            for (var i = 0; i < userIds.Length; i++)
            {
                var userId = userIds[i];
                var user = users[i];
                backOfficeUserManager.Setup(x => x.FindByIdAsync(userId.ToString()))
                    .ReturnsAsync(user);
                backOfficeUserManager.Setup(x => x.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset?>()))
                    .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = expectedMessage }));
            }

            var actual = sut.PostUnlockUsers(userIds);

            //
            //
            // Assert.Multiple(() =>
            // {
            //
            // });
        }
    }
}
