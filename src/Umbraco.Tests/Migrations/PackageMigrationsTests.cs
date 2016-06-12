using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Packaging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.Services;
using Umbraco.Tests.Migrations.Stubs;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Install.MigrationSteps;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;

namespace Umbraco.Tests.Migrations
{
    [DatabaseTestBehavior(DatabaseBehavior.EmptyDbFilePerTest)]
    [TestFixture]
    public class PackageMigrationsTests : BaseDatabaseFactoryTest
    {
        private DatabaseContext _databaseContext;
        private ILogger _logger;
        private IMigrationEntryService _migrationEntryService;

        /// <summary>
        /// sets up resolvers before resolution is frozen
        /// </summary>
        protected override void FreezeResolution()
        {
            MigrationResolver.Current = new MigrationResolver(
                Mock.Of<ILogger>(),
                () => new List<Type>
                {
                    typeof (PackageMigration1),
                    typeof (PackageMigration2)
                });
            
            //This is needed because the Migration resolver is creating the migration instances with their full ctors
            _logger = Mock.Of<ILogger>();
            LoggerResolver.Current = new LoggerResolver(_logger)
            {
                CanResolveBeforeFrozen = true
            };

            base.FreezeResolution();
        }

        protected override ApplicationContext CreateApplicationContext()
        {
            var sqlSyntax = new SqlCeSyntaxProvider();
            var db = GetConfiguredDatabase();
            
            var databaseContext = new Mock<DatabaseContext>(Mock.Of<IDatabaseFactory>(), Mock.Of<ILogger>(), sqlSyntax, "test");
            databaseContext.SetupGet(x => x.CanConnect).Returns(true);
            databaseContext.SetupGet(x => x.IsDatabaseConfigured).Returns(true);
            databaseContext.Setup(x => x.Database).Returns(db);
            _databaseContext = databaseContext.Object;

            var umbracoMigrationEntry = new MigrationEntry(1, DateTime.UtcNow, GlobalSettings.UmbracoMigrationName,
                UmbracoVersion.GetSemanticVersion());

            var migrationEntryService = new Mock<IMigrationEntryService>();
            migrationEntryService.Setup(x => x.FindEntry(It.IsAny<string>(), It.IsAny<SemVersion>()))
                .Returns(umbracoMigrationEntry);
            migrationEntryService.Setup(x => x.FindEntries(It.IsAny<string>())).Returns(Enumerable.Empty<IMigrationEntry>());
            migrationEntryService.Setup(x => x.FindEntries(It.IsAny<string[]>())).Returns(Enumerable.Empty<IMigrationEntry>());
            _migrationEntryService = migrationEntryService.Object;

            //This is needed because the Migration resolver is creating migration instances with their full ctors
            var applicationContext = new ApplicationContext(
                //assign the db context
                _databaseContext,
                //assign the service context
                new ServiceContext(migrationEntryService: _migrationEntryService),
                CacheHelper.CreateDisabledCacheHelper(),
                ProfilingLogger)
            {
                IsReady = true
            };

            return applicationContext;
        }

        [Test]
        public void PackageMigrationsContext_HasPendingPackageMigrations()
        {
            var packageMigrationsContext = new PackageMigrationsContext(_databaseContext, 
                _migrationEntryService, _logger);
            var hasPackageMigrations = packageMigrationsContext.HasPendingPackageMigrations;
            Assert.True(hasPackageMigrations);

            var result = packageMigrationsContext.GetPendingPackageMigrations();
            Assert.AreEqual(1, result.Count);
            Assert.True(result.ContainsKey("com.company.package"));
            Assert.AreEqual(new SemVersion(2, 0, 0), result.First().Value);

            var names = packageMigrationsContext.GetPendingPackageMigrationFriendlyNames();
            var friendlyName = names.First();
            Assert.AreEqual("com.company.package (2.0.0)", friendlyName);
        }

        [Test]
        public void ExecutionMigrationStep_Can_Execute()
        {
            var packageMigrationsContext = new PackageMigrationsContext(_databaseContext,
                _migrationEntryService, _logger);
            var migrationStep = new ExecutionMigrationStep(_migrationEntryService, packageMigrationsContext, 
                _databaseContext.Database, _databaseContext.DatabaseProvider, _logger);
            //Note: model isn't used here, so we just pass in an empty string
            var executed = migrationStep.Execute(string.Empty);
            Assert.Null(executed);
        }

        public UmbracoDatabase GetConfiguredDatabase()
        {
            return new UmbracoDatabase(GetDbConnectionString(), GetDbProviderName(), Mock.Of<ILogger>());
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            LoggerResolver.Reset();
            MigrationResolver.Reset();
        }
    }
}