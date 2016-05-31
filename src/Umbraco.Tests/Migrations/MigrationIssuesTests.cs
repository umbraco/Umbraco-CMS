using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven;
using Umbraco.Tests.TestHelpers;

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
                Text = "text"
            };
            DatabaseContext.Database.Insert(data);
            data = new PropertyDataDto
            {
                NodeId = n.NodeId,
                PropertyTypeId = pt.Id,
                Text = "<root><node title=\"\" type=\"\" newwindow=\"\" link=\"\" /></root>"
            };
            DatabaseContext.Database.Insert(data);
            var migrationContext = new MigrationContext(DatabaseContext.Database, Logger);

            var migration = new UpdateRelatedLinksData(migrationContext);
            migration.UpdateRelatedLinksDataDo(DatabaseContext.Database);

            data = DatabaseContext.Database.Fetch<PropertyDataDto>("SELECT * FROM cmsPropertyData WHERE id=" + data.Id).FirstOrDefault();
            Assert.IsNotNull(data);
            Console.WriteLine(data.Text);
            Assert.AreEqual("[{\"title\":\"\",\"caption\":\"\",\"link\":\"\",\"newWindow\":false,\"type\":\"external\",\"internal\":null,\"edit\":false,\"isInternal\":false}]",
                data.Text);
        }
    }
}