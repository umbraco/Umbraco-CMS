using System;
using System.Configuration;
using Moq;
using NUnit.Framework;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.Services;

namespace Umbraco.Tests
{
    [TestFixture]
    public class ApplicationContextTests
    {
        [Test]
        public void Is_Configured()
        {
            ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", UmbracoVersion.GetSemanticVersion().ToSemanticString());

            var migrationEntryService = new Mock<IMigrationEntryService>();
            migrationEntryService.Setup(x => x.FindEntry(It.IsAny<string>(), It.IsAny<SemVersion>()))
                .Returns(Mock.Of<IMigrationEntry>());

            var dbCtx = new Mock<DatabaseContext>(Mock.Of<IDatabaseFactory>(), Mock.Of<ILogger>(), new SqlCeSyntaxProvider(), "test");
            dbCtx.Setup(x => x.IsDatabaseConfigured).Returns(true);
            dbCtx.Setup(x => x.CanConnect).Returns(true);

            var appCtx = new ApplicationContext(
                dbCtx.Object,
                new ServiceContext(migrationEntryService:migrationEntryService.Object), 
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            Assert.IsTrue(appCtx.IsConfigured);
        }

        [Test]
        public void Is_Not_Configured_By_Migration_Not_Found()
        {
            ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", UmbracoVersion.GetSemanticVersion().ToSemanticString());

            var migrationEntryService = new Mock<IMigrationEntryService>();
            migrationEntryService.Setup(x => x.FindEntry(It.IsAny<string>(), It.IsAny<SemVersion>()))
                .Returns((IMigrationEntry)null);

            var dbCtx = new Mock<DatabaseContext>(Mock.Of<IDatabaseFactory>(), Mock.Of<ILogger>(), new SqlCeSyntaxProvider(), "test");
            dbCtx.Setup(x => x.IsDatabaseConfigured).Returns(true);
            dbCtx.Setup(x => x.CanConnect).Returns(true);

            var appCtx = new ApplicationContext(
                dbCtx.Object,
                new ServiceContext(migrationEntryService: migrationEntryService.Object),
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            Assert.IsFalse(appCtx.IsConfigured);
        }

        [Test]
        public void Is_Not_Configured_By_Configuration()
        {
            ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", new SemVersion(UmbracoVersion.Current.Major - 1, 0, 0).ToString());

            var migrationEntryService = new Mock<IMigrationEntryService>();

            var dbCtx = new Mock<DatabaseContext>(Mock.Of<IDatabaseFactory>(), Mock.Of<ILogger>(), new SqlCeSyntaxProvider(), "test");
            dbCtx.Setup(x => x.IsDatabaseConfigured).Returns(true);
            dbCtx.Setup(x => x.CanConnect).Returns(true);

            var appCtx = new ApplicationContext(
                dbCtx.Object,
                new ServiceContext(migrationEntryService: migrationEntryService.Object),
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            Assert.IsFalse(appCtx.IsConfigured);
        }

        [Test]
        public void Is_Not_Configured_By_Database_Not_Configured()
        {
            ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", new SemVersion(UmbracoVersion.Current.Major - 1, 0, 0).ToString());

            var migrationEntryService = new Mock<IMigrationEntryService>();

            var dbCtx = new Mock<DatabaseContext>(Mock.Of<IDatabaseFactory>(), Mock.Of<ILogger>(), new SqlCeSyntaxProvider(), "test");
            dbCtx.Setup(x => x.IsDatabaseConfigured).Returns(false);
            dbCtx.Setup(x => x.CanConnect).Returns(true);

            var appCtx = new ApplicationContext(
                dbCtx.Object,
                new ServiceContext(migrationEntryService: migrationEntryService.Object),
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            Assert.IsFalse(appCtx.IsConfigured);
        }

        [Test]
        public void Is_Not_Configured_By_Database_Cannot_Connect()
        {
            ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", new SemVersion(UmbracoVersion.Current.Major - 1, 0, 0).ToString());

            var migrationEntryService = new Mock<IMigrationEntryService>();

            var dbCtx = new Mock<DatabaseContext>(Mock.Of<IDatabaseFactory>(), Mock.Of<ILogger>(), new SqlCeSyntaxProvider(), "test");
            dbCtx.Setup(x => x.IsDatabaseConfigured).Returns(true);
            dbCtx.Setup(x => x.CanConnect).Returns(false);

            var appCtx = new ApplicationContext(
                dbCtx.Object,
                new ServiceContext(migrationEntryService: migrationEntryService.Object),
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            Assert.IsFalse(appCtx.IsConfigured);
        }
    }
}