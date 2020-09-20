using NUnit.Framework;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class WebRoutingSettingsBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            const bool disableAlternativeTemplates = true;
            const bool disableFindContentByIdPath = true;
            const bool disableRedirectUrlTracking = true;
            const bool internalRedirectPreservesTemplate = true;
            const bool trySkipIisCustomErrors = true;
            const string umbracoApplicationUrl = "/test/";
            const string urlProviderMode = "test";
            const bool validateAlternativeTemplates = true;

            var builder = new WebRoutingSettingsBuilder();

            // Act
            var webRoutingSettings = builder
                .WithDisableAlternativeTemplates(disableAlternativeTemplates)
                .WithDisableFindContentByIdPath(disableFindContentByIdPath)
                .WithDisableRedirectUrlTracking(disableRedirectUrlTracking)
                .WithInternalRedirectPreservesTemplate(internalRedirectPreservesTemplate)
                .WithTrySkipIisCustomErrors(trySkipIisCustomErrors)
                .WithUmbracoApplicationUrl(umbracoApplicationUrl)
                .WithUrlProviderMode(urlProviderMode)
                .WithValidateAlternativeTemplates(validateAlternativeTemplates)
                .Build();

            // Assert
            Assert.AreEqual(disableAlternativeTemplates, webRoutingSettings.DisableAlternativeTemplates);
            Assert.AreEqual(disableFindContentByIdPath, webRoutingSettings.DisableFindContentByIdPath);
            Assert.AreEqual(disableRedirectUrlTracking, webRoutingSettings.DisableRedirectUrlTracking);
            Assert.AreEqual(internalRedirectPreservesTemplate, webRoutingSettings.InternalRedirectPreservesTemplate);
            Assert.AreEqual(trySkipIisCustomErrors, webRoutingSettings.TrySkipIisCustomErrors);
            Assert.AreEqual(umbracoApplicationUrl, webRoutingSettings.UmbracoApplicationUrl);
            Assert.AreEqual(urlProviderMode, webRoutingSettings.UrlProviderMode);
            Assert.AreEqual(validateAlternativeTemplates, webRoutingSettings.ValidateAlternativeTemplates);
        }
    }
}
