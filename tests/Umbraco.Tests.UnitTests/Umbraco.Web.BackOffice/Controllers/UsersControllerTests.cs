// Copyright (c) Umbraco.
// See LICENSE for more details.

using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Controllers;

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

        var result = sut.PostUnlockUsers(userIds).Result as ObjectResult;
        Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
    }
}
