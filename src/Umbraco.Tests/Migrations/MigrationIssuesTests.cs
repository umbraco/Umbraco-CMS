using System;
using System.Diagnostics;
using System.Linq;
using Moq;
using NUnit.Framework;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFiveZero;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    public class MigrationIssuesTests : BaseDatabaseFactoryTest
    {
        [Test]
        public void Issue8370Test()
        {
            // fixme maybe we need to create some content?
            // yes otherwise cannot get it to fail!

            var n = new NodeDto
            {
                Text = "text",
                CreateDate = DateTime.Now,
                Path = "-1",
                ParentId = -1,
                UniqueId = Guid.NewGuid()
            };
            DatabaseContext.Database.Insert(n);
            var ct = new ContentTypeDto
            {
                Alias = "alias",
                NodeId = n.NodeId,
                Thumbnail = "thumb"
            };
            DatabaseContext.Database.Insert(ct);
            n = new NodeDto
            {
                Text = "text",
                CreateDate = DateTime.Now,
                Path = "-1",
                ParentId = -1,
                UniqueId = Guid.NewGuid()
            };
            DatabaseContext.Database.Insert(n);
            var dt = new DataTypeDto
            {
                PropertyEditorAlias = Constants.PropertyEditors.RelatedLinksAlias,
                DbType = "x",
                DataTypeId = n.NodeId
            };
            DatabaseContext.Database.Insert(dt);
            var pt = new PropertyTypeDto
            {
                Alias = "alias",
                ContentTypeId = ct.NodeId,
                DataTypeId = dt.DataTypeId
            };
            DatabaseContext.Database.Insert(pt);
            n = new NodeDto
            {
                Text = "text",
                CreateDate = DateTime.Now,
                Path = "-1",
                ParentId = -1,
                UniqueId = Guid.NewGuid()
            };
            DatabaseContext.Database.Insert(n);
            var data = new PropertyDataDto
            {
                NodeId = n.NodeId,
                PropertyTypeId = pt.Id,
                Text = "text",
                VersionId = Guid.NewGuid()
            };
            DatabaseContext.Database.Insert(data);
            data = new PropertyDataDto
            {
                NodeId = n.NodeId,
                PropertyTypeId = pt.Id,
                Text = "<root><node title=\"\" type=\"\" newwindow=\"\" link=\"\" /></root>",
                VersionId = Guid.NewGuid()
            };
            DatabaseContext.Database.Insert(data);

            var migration = new UpdateRelatedLinksData(SqlSyntax, Logger);
            migration.UpdateRelatedLinksDataDo(DatabaseContext.Database);

            data = DatabaseContext.Database.Fetch<PropertyDataDto>("SELECT * FROM cmsPropertyData WHERE id=" + data.Id).FirstOrDefault();
            Assert.IsNotNull(data);
            Debug.Print(data.Text);
            Assert.AreEqual("[{\"title\":\"\",\"caption\":\"\",\"link\":\"\",\"newWindow\":false,\"type\":\"external\",\"internal\":null,\"edit\":false,\"isInternal\":false}]",
                data.Text);
        }

        [Test]
        public void Issue8361Test()
        {
            var logger = new DebugDiagnosticsLogger();

            //Setup the MigrationRunner
            var migrationRunner = new MigrationRunner(
                Mock.Of<IMigrationEntryService>(),
                logger,
                new SemVersion(7, 4, 0),
                new SemVersion(7, 5, 0),
                Constants.System.UmbracoMigrationName,

                //pass in explicit migrations
                new DeleteRedirectUrlTable(SqlSyntax, logger),
                new AddRedirectUrlTable(SqlSyntax, logger)
            );

            var db = new UmbracoDatabase("Datasource=|DataDirectory|UmbracoPetaPocoTests.sdf;Flush Interval=1;", Constants.DatabaseProviders.SqlCe, Logger);

            var upgraded = migrationRunner.Execute(db, DatabaseProviders.SqlServerCE, true);
            Assert.IsTrue(upgraded);
        }

        [Migration("7.5.0", 99, Constants.System.UmbracoMigrationName)]
        public class DeleteRedirectUrlTable : MigrationBase
        {
            public DeleteRedirectUrlTable(ISqlSyntaxProvider sqlSyntax, ILogger logger)
                : base(sqlSyntax, logger)
            { }

            public override void Up()
            {
                Delete.Table("umbracoRedirectUrl");
            }

            public override void Down()
            { }
        }
    }
}