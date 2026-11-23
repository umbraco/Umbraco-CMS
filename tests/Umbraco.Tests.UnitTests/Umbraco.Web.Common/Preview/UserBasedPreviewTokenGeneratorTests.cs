// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Preview;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Preview;

[TestFixture]
#pragma warning disable CS0618 // Type or member is obsolete - all usage has been removed up in V19
public class UserBasedPreviewTokenGeneratorTests
{
    private readonly IDataProtectionProvider _dataProtectionProvider = new EphemeralDataProtectionProvider();
    private Mock<IUser> _userMock = null!;
    private Mock<IUserService> _userServiceMock = null!;

    [SetUp]
    public void SetUp()
    {
        _userMock = new Mock<IUser>();
        _userMock.SetupGet(x => x.Key).Returns(Guid.NewGuid());
        _userMock.SetupGet(x => x.IsApproved).Returns(true);
        _userMock.SetupGet(x => x.IsLockedOut).Returns(false);
        _userMock.SetupGet(x => x.SecurityStamp).Returns(Guid.NewGuid().ToString());

        _userServiceMock = new Mock<IUserService>();
        _userServiceMock
            .Setup(x => x.GetAsync(_userMock.Object.Key))
            .ReturnsAsync(() => _userMock.Object);
    }

    [Test]
    public async Task VerifyAsync_Succeeds_For_Generated_Token()
    {
        UserBasedPreviewTokenGenerator generator = CreateGenerator();

        var token = await GenerateTokenAsync(generator);
        Attempt<Guid?> result = await generator.VerifyAsync(token);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(_userMock.Object.Key, result.Result);
    }

    [Test]
    public async Task VerifyAsync_Fails_For_Expired_Token()
    {
        UserBasedPreviewTokenGenerator generator = CreateGenerator(TimeSpan.FromMilliseconds(1));

        var token = await GenerateTokenAsync(generator);
        await Task.Delay(TimeSpan.FromMilliseconds(50));
        Attempt<Guid?> result = await generator.VerifyAsync(token);

        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task VerifyAsync_Fails_When_Security_Stamp_Has_Changed()
    {
        UserBasedPreviewTokenGenerator generator = CreateGenerator();

        var token = await GenerateTokenAsync(generator);
        _userMock.SetupGet(x => x.SecurityStamp).Returns(Guid.NewGuid().ToString());
        Attempt<Guid?> result = await generator.VerifyAsync(token);

        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task VerifyAsync_Fails_For_Token_Carrying_Only_The_User_Key()
    {
        UserBasedPreviewTokenGenerator generator = CreateGenerator();

        var token = EncryptionHelper.Encrypt(_userMock.Object.Key.ToString(), _dataProtectionProvider);
        Attempt<Guid?> result = await generator.VerifyAsync(token);

        Assert.IsFalse(result.Success);
    }

    [TestCase(false, false)]
    [TestCase(true, true)]
    public async Task VerifyAsync_Fails_For_Unapproved_Or_Locked_Out_User(bool isApproved, bool isLockedOut)
    {
        UserBasedPreviewTokenGenerator generator = CreateGenerator();

        var token = await GenerateTokenAsync(generator);
        _userMock.SetupGet(x => x.IsApproved).Returns(isApproved);
        _userMock.SetupGet(x => x.IsLockedOut).Returns(isLockedOut);
        Attempt<Guid?> result = await generator.VerifyAsync(token);

        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task GenerateTokenAsync_Fails_For_Unknown_User()
    {
        UserBasedPreviewTokenGenerator generator = CreateGenerator();

        Attempt<string?> result = await generator.GenerateTokenAsync(Guid.NewGuid());

        Assert.IsFalse(result.Success);
    }

    private async Task<string> GenerateTokenAsync(UserBasedPreviewTokenGenerator generator)
    {
        Attempt<string?> attempt = await generator.GenerateTokenAsync(_userMock.Object.Key);
        Assert.IsTrue(attempt.Success);
        return attempt.Result!;
    }

    private UserBasedPreviewTokenGenerator CreateGenerator(TimeSpan? lifetime = null)
        => new(
            _dataProtectionProvider,
            _userServiceMock.Object,
            NullLogger<UserBasedPreviewTokenGenerator>.Instance,
            Options.Create(new GlobalSettings { TimeOut = lifetime ?? TimeSpan.FromMinutes(20) }));
}
#pragma warning restore CS0618 // Type or member is obsolete
