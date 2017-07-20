using System;
using System.Diagnostics;
using System.Linq;
using Moq;
using NUnit.Framework;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionEight;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class MigrationIssuesTests : TestWithDatabaseBase
    {
        [Test]
        public void Issue8370Test()
        {
            // make sure to create some content,
            // otherwise cannot get it to fail!

            using (var scope = Current.ScopeProvider.CreateScope())
            {
                var database = scope.Database;

                var n = new NodeDto
                {
                    Text = "text",
                    CreateDate = DateTime.Now,
                    Path = "-1",
                    ParentId = -1,
                    UniqueId = Guid.NewGuid()
                };
                database.Insert(n);
                var ct = new ContentTypeDto
                {
                    Alias = "alias",
                    NodeId = n.NodeId,
                    Thumbnail = "thumb"
                };
                database.Insert(ct);
                n = new NodeDto
                {
                    Text = "text",
                    CreateDate = DateTime.Now,
                    Path = "-1",
                    ParentId = -1,
                    UniqueId = Guid.NewGuid()
                };
                database.Insert(n);
                var dt = new DataTypeDto
                {
                    PropertyEditorAlias = Constants.PropertyEditors.RelatedLinksAlias,
                    DbType = "x",
                    DataTypeId = n.NodeId
                };
                database.Insert(dt);
                var pt = new PropertyTypeDto
                {
                    Alias = "alias",
                    ContentTypeId = ct.NodeId,
                    DataTypeId = dt.DataTypeId
                };
                database.Insert(pt);
                n = new NodeDto
                {
                    Text = "text",
                    CreateDate = DateTime.Now,
                    Path = "-1",
                    ParentId = -1,
                    UniqueId = Guid.NewGuid()
                };
                database.Insert(n);
                var data = new PropertyDataDto
                {
                    NodeId = n.NodeId,
                    PropertyTypeId = pt.Id,
                    Text = "text",
                    VersionId = Guid.NewGuid()
                };
                database.Insert(data);
                data = new PropertyDataDto
                {
                    NodeId = n.NodeId,
                    PropertyTypeId = pt.Id,
                    Text = "<root><node title=\"\" type=\"\" newwindow=\"\" link=\"\" /></root>",
                    VersionId = Guid.NewGuid()
                };
                database.Insert(data);
                var migrationContext = new MigrationContext(database, Logger);

                var migration = new UpdateRelatedLinksData(migrationContext);
                migration.UpdateRelatedLinksDataDo(database);

                data = database.Fetch<PropertyDataDto>("SELECT * FROM cmsPropertyData WHERE id=" + data.Id).FirstOrDefault();
                Assert.IsNotNull(data);
                Debug.Print(data.Text);
                Assert.AreEqual("[{\"title\":\"\",\"caption\":\"\",\"link\":\"\",\"newWindow\":false,\"type\":\"external\",\"internal\":null,\"edit\":false,\"isInternal\":false}]",
                    data.Text);
            }
        }

        [Test]
        public void Issue8361Test()
        {
            var logger = new DebugDiagnosticsLogger();

            using (var scope = Current.ScopeProvider.CreateScope())
            {
                var database = scope.Database;

                var migrationContext = new MigrationContext(database, Logger);

                //Setup the MigrationRunner
                var migrationRunner = new MigrationRunner(
                    Mock.Of<IMigrationCollectionBuilder>(),
                    Mock.Of<IMigrationEntryService>(),
                    logger,
                    new SemVersion(7, 5, 0),
                    new SemVersion(8, 0, 0),
                    Constants.System.UmbracoMigrationName,

                    //pass in explicit migrations
                    new DeleteRedirectUrlTable(migrationContext),
                    new AddRedirectUrlTable(migrationContext)
                );

                var upgraded = migrationRunner.Execute(migrationContext, true);
                Assert.IsTrue(upgraded);
            }
        }

        [Migration("8.0.0", 99, Constants.System.UmbracoMigrationName)]
        public class DeleteRedirectUrlTable : MigrationBase
        {
            public DeleteRedirectUrlTable(IMigrationContext context)
                : base(context)
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
