using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Security;

[TestFixture]
public class HmacSecretKeyServiceTests
{
    [Test]
    public void HasHmacSecretKey_ReturnsFalse_WhenEmpty()
    {
        HmacSecretKeyService sut = CreateService([]);

        Assert.IsFalse(sut.HasHmacSecretKey());
    }

    [Test]
    public void HasHmacSecretKey_ReturnsTrue_WhenSet()
    {
        HmacSecretKeyService sut = CreateService([1, 2, 3]);

        Assert.IsTrue(sut.HasHmacSecretKey());
    }

    [Test]
    public async Task CreateHmacSecretKeyAsync_GeneratesAndPersists()
    {
        var configManipulatorMock = new Mock<IConfigManipulator>();
        HmacSecretKeyService sut = CreateService([], configManipulatorMock);

        Attempt<HmacSecretKeyOperationStatus> result = await sut.CreateHmacSecretKeyAsync();

        Assert.IsTrue(result.Success);
        Assert.AreEqual(HmacSecretKeyOperationStatus.Success, result.Result);
        configManipulatorMock.Verify(
            x => x.SetImagingHmacSecretKeyAsync(It.Is<string>(key =>
                Convert.FromBase64String(key).Length == 64)),
            Times.Once);
    }

    [Test]
    public async Task CreateHmacSecretKeyAsync_ReturnsKeyExists_WhenKeyAlreadyExists()
    {
        var configManipulatorMock = new Mock<IConfigManipulator>();
        HmacSecretKeyService sut = CreateService([1, 2, 3], configManipulatorMock);

        Attempt<HmacSecretKeyOperationStatus> result = await sut.CreateHmacSecretKeyAsync();

        Assert.IsFalse(result.Success);
        Assert.AreEqual(HmacSecretKeyOperationStatus.KeyExists, result.Result);
        configManipulatorMock.Verify(
            x => x.SetImagingHmacSecretKeyAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task CreateHmacSecretKeyAsync_ReturnsError_OnException()
    {
        var configManipulatorMock = new Mock<IConfigManipulator>();
        configManipulatorMock
            .Setup(x => x.SetImagingHmacSecretKeyAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        HmacSecretKeyService sut = CreateService([], configManipulatorMock);

        Attempt<HmacSecretKeyOperationStatus> result = await sut.CreateHmacSecretKeyAsync();

        Assert.IsFalse(result.Success);
        Assert.AreEqual(HmacSecretKeyOperationStatus.Error, result.Result);
        Assert.IsNotNull(result.Exception);
    }

    private static HmacSecretKeyService CreateService(
        byte[] hmacSecretKey,
        Mock<IConfigManipulator>? configManipulatorMock = null)
    {
        var imagingSettings = new ImagingSettings { HMACSecretKey = hmacSecretKey };
        var optionsMonitor = Mock.Of<IOptionsMonitor<ImagingSettings>>(x => x.CurrentValue == imagingSettings);

        return new HmacSecretKeyService(
            optionsMonitor,
            (configManipulatorMock ?? new Mock<IConfigManipulator>()).Object,
            Mock.Of<ILogger<HmacSecretKeyService>>());
    }
}
