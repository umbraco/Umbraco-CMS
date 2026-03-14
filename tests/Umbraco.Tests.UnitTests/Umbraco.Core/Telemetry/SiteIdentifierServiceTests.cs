using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Telemetry;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Telemetry;

/// <summary>
/// Contains unit tests for the <see cref="SiteIdentifierService"/> class, verifying its functionality and behavior.
/// </summary>
[TestFixture]
public class SiteIdentifierServiceTests
{
    /// <summary>
    /// Tests that TryGetSiteIdentifier only succeeds if the provided guidString is a valid GUID.
    /// </summary>
    /// <param name="guidString">The GUID string to test.</param>
    /// <param name="shouldSucceed">Indicates whether the TryGetSiteIdentifier call is expected to succeed.</param>
    [TestCase("0F1785C5-7BA0-4C52-AB62-863BD2C8F3FE", true)]
    [TestCase("This is not a guid", false)]
    [TestCase("", false)]
    [TestCase("00000000-0000-0000-0000-000000000000", false)] // Don't count empty GUID as valid
    public void TryGetOnlyPassesIfValidId(string guidString, bool shouldSucceed)
    {
        var globalSettings = CreateGlobalSettings(guidString);
        var sut = new SiteIdentifierService(
            globalSettings,
            Mock.Of<IConfigManipulator>(),
            Mock.Of<ILogger<SiteIdentifierService>>());

        var result = sut.TryGetSiteIdentifier(out var siteIdentifier);

        Assert.AreEqual(shouldSucceed, result);
        if (shouldSucceed)
        {
            // When toString is called on a GUID it will to lower, so do the same to our guidString
            Assert.AreEqual(guidString.ToLower(), siteIdentifier.ToString());
        }
        else
        {
            Assert.AreEqual(Guid.Empty, siteIdentifier);
        }
    }

    /// <summary>
    /// Tests that a new GUID is only created if the current GUID is missing or invalid.
    /// </summary>
    /// <param name="guidString">The GUID string to test with.</param>
    /// <param name="shouldCreate">Indicates whether a new GUID should be created.</param>
    [TestCase("0F1785C5-7BA0-4C52-AB62-863BD2C8F3FE", false)]
    [TestCase("This is not a guid", true)]
    [TestCase("", true)]
    [TestCase("00000000-0000-0000-0000-000000000000", true)] // Don't count empty GUID as valid
    public void TryGetOrCreateOnlyCreatesNewGuidIfCurrentIsMissingOrInvalid(string guidString, bool shouldCreate)
    {
        var globalSettings = CreateGlobalSettings(guidString);
        var configManipulatorMock = new Mock<IConfigManipulator>();

        var sut = new SiteIdentifierService(
            globalSettings,
            configManipulatorMock.Object,
            Mock.Of<ILogger<SiteIdentifierService>>());

        var result = sut.TryGetOrCreateSiteIdentifier(out var identifier);

        if (shouldCreate)
        {
            configManipulatorMock.Verify(x => x.SetGlobalIdAsync(It.IsAny<string>()), Times.Once);
            Assert.AreNotEqual(Guid.Empty, identifier);
            Assert.IsTrue(result);
        }
        else
        {
            configManipulatorMock.Verify(x => x.SetGlobalIdAsync(It.IsAny<string>()), Times.Never());
            Assert.AreEqual(guidString.ToLower(), identifier.ToString());
            Assert.IsTrue(result);
        }
    }

    private IOptionsMonitor<GlobalSettings> CreateGlobalSettings(string guidString)
    {
        var globalSettings = new GlobalSettings { Id = guidString };
        return Mock.Of<IOptionsMonitor<GlobalSettings>>(x => x.CurrentValue == globalSettings);
    }
}
