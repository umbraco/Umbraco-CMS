using System.Net.Mail;
using NUnit.Framework;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class GlobalSettingsBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            const string configurationStatus = "9.0.1";
            const string databaseFactoryServerVersion = "testServerVersion";
            const string defaultUiLanguage = "en-GB";
            const bool disableElectionForSingleServer = true;
            const bool hideTopLevelNodeFromPath = true;
            const bool installEmptyDatabase = true;
            const bool installMissingDatabase = true;
            const string umbracoPath = "~/testumbraco";
            const string registerType = "testRegisterType";
            const string reservedPaths = "/custom";
            const string reservedUrls = "~/custom,";
            const int timeOutInMinutes = 20;
            const string umbracoCssPath = "~/testcss";
            const string umbracoMediaPath = "~/testmedia";
            const string umbracoScriptsPath = "~/testscripts";
            const string mainDomLock = "testMainDomLock";
            const string noNodesViewPath = "~/config/splashes/MyNoNodes.cshtml";
            const bool useHttps = true;
            const int versionCheckPeriod = 1;
            const string smtpFrom = "test@test.com";
            const string smtpHost = "test.com";
            const int smtpPort = 225;
            const SmtpDeliveryMethod smtpDeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
            const string smtpPickupDirectoryLocation = "c:\\test";
            const string smtpUsername = "testuser";
            const string smtpPassword = "testpass";

            var builder = new GlobalSettingsBuilder();

            // Act
            var globalSettings = builder
                .WithConfigurationStatus(configurationStatus)
                .WithDatabaseFactoryServerVersion(databaseFactoryServerVersion)
                .WithDefaultUiLanguage(defaultUiLanguage)
                .WithDisableElectionForSingleServer(disableElectionForSingleServer)
                .WithHideTopLevelNodeFromPath(hideTopLevelNodeFromPath)
                .WithInstallEmptyDatabase(installEmptyDatabase)
                .WithInstallMissingDatabase(installMissingDatabase)
                .WithUmbracoPath(umbracoPath)
                .WithRegisterType(registerType)
                .WithReservedPaths(reservedPaths)
                .WithReservedUrls(reservedUrls)
                .WithTimeOutInMinutes(timeOutInMinutes)
                .WithUmbracoCssPath(umbracoCssPath)
                .WithUmbracoMediaPath(umbracoMediaPath)
                .WithUmbracoScriptsPath(umbracoScriptsPath)
                .WithMainDomLock(mainDomLock)
                .WithNoNodesViewPath(noNodesViewPath)
                .WithUseHttps(useHttps)
                .WithVersionCheckPeriod(versionCheckPeriod)
                .AddSmtpSettings()
                    .WithFrom(smtpFrom)
                    .WithHost(smtpHost)
                    .WithPort(smtpPort)
                    .WithDeliveryMethod(smtpDeliveryMethod.ToString())
                    .WithPickupDirectoryLocation(smtpPickupDirectoryLocation)
                    .WithUsername(smtpUsername)
                    .WithPassword(smtpPassword)
                    .Done()
                .Build();

            // Assert
            Assert.AreEqual(configurationStatus, globalSettings.ConfigurationStatus);
            Assert.AreEqual(databaseFactoryServerVersion, globalSettings.DatabaseFactoryServerVersion);
            Assert.AreEqual(defaultUiLanguage, globalSettings.DefaultUILanguage);
            Assert.AreEqual(disableElectionForSingleServer, globalSettings.DisableElectionForSingleServer);
            Assert.AreEqual(hideTopLevelNodeFromPath, globalSettings.HideTopLevelNodeFromPath);
            Assert.AreEqual(installEmptyDatabase, globalSettings.InstallEmptyDatabase);
            Assert.AreEqual(installMissingDatabase, globalSettings.InstallMissingDatabase);
            Assert.AreEqual(umbracoPath, globalSettings.UmbracoPath);
            Assert.AreEqual(registerType, globalSettings.RegisterType);
            Assert.AreEqual(reservedPaths, globalSettings.ReservedPaths);
            Assert.AreEqual(reservedUrls, globalSettings.ReservedUrls);
            Assert.AreEqual(timeOutInMinutes, globalSettings.TimeOutInMinutes);
            Assert.AreEqual(umbracoCssPath, globalSettings.UmbracoCssPath);
            Assert.AreEqual(umbracoMediaPath, globalSettings.UmbracoMediaPath);
            Assert.AreEqual(umbracoScriptsPath, globalSettings.UmbracoScriptsPath);
            Assert.AreEqual(mainDomLock, globalSettings.MainDomLock);
            Assert.AreEqual(noNodesViewPath, globalSettings.NoNodesViewPath);
            Assert.AreEqual(useHttps, globalSettings.UseHttps);
            Assert.AreEqual(versionCheckPeriod, globalSettings.VersionCheckPeriod);
            Assert.AreEqual(smtpFrom, globalSettings.Smtp.From);
            Assert.AreEqual(smtpHost, globalSettings.Smtp.Host);
            Assert.AreEqual(smtpPort, globalSettings.Smtp.Port);
            Assert.AreEqual(smtpDeliveryMethod, globalSettings.Smtp.DeliveryMethodValue);
            Assert.AreEqual(smtpPickupDirectoryLocation, globalSettings.Smtp.PickupDirectoryLocation);
            Assert.AreEqual(smtpUsername, globalSettings.Smtp.Username);
            Assert.AreEqual(smtpPassword, globalSettings.Smtp.Password);
        }
    }
}
