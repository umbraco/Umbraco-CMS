using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
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
        HmacSecretKeyService sut = CreateService(Array.Empty<byte>());

        Assert.IsFalse(sut.HasHmacSecretKey());
    }

    [Test]
    public void HasHmacSecretKey_ReturnsTrue_WhenSet()
    {
        HmacSecretKeyService sut = CreateService(new byte[] { 1, 2, 3 });

        Assert.IsTrue(sut.HasHmacSecretKey());
    }

    [Test]
    public async Task TryCreateHmacSecretKeyAsync_GeneratesAndPersists()
    {
        var configManipulatorMock = new Mock<IConfigManipulator>();
        HmacSecretKeyService sut = CreateService(Array.Empty<byte>(), configManipulatorMock);

        var result = await sut.TryCreateHmacSecretKeyAsync();

        Assert.IsTrue(result);
        configManipulatorMock.Verify(
            x => x.SetImagingHmacSecretKeyAsync(It.Is<string>(key =>
                Convert.FromBase64String(key).Length == 64)),
            Times.Once);
    }

    [Test]
    public async Task TryCreateHmacSecretKeyAsync_ReturnsFalse_OnException()
    {
        var configManipulatorMock = new Mock<IConfigManipulator>();
        configManipulatorMock
            .Setup(x => x.SetImagingHmacSecretKeyAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        HmacSecretKeyService sut = CreateService(Array.Empty<byte>(), configManipulatorMock);

        var result = await sut.TryCreateHmacSecretKeyAsync();

        Assert.IsFalse(result);
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
