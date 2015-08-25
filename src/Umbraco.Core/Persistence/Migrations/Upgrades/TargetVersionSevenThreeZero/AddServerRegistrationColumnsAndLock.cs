using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 17, GlobalSettings.UmbracoMigrationName)]
    public class AddServerRegistrationColumnsAndLock : MigrationBase
    {
        public AddServerRegistrationColumnsAndLock(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            // don't execute if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();
            if (columns.Any(x => x.TableName.InvariantEquals("umbracoServer") && x.ColumnName.InvariantEquals("isMaster")) == false)
            {
                Create.Column("isMaster").OnTable("umbracoServer").AsBoolean().NotNullable().WithDefaultValue(0);
            }

            // wrap in a transaction so that everything runs on the same connection
            // and the IDENTITY_INSERT stuff is effective for all inserts.
            using (var tr = Context.Database.GetTransaction())
            {
                // turn on identity insert if db provider is not mysql
                if (SqlSyntax.SupportsIdentityInsert())
                    Context.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON", SqlSyntax.GetQuotedTableName("umbracoNode"))));

                InsertLockObject(Constants.System.ServersLock, "0AF5E610-A310-4B6F-925F-E928D5416AF7", "LOCK: Servers");

                // turn off identity insert if db provider is not mysql
                if (SqlSyntax.SupportsIdentityInsert())
                    Context.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF", SqlSyntax.GetQuotedTableName("umbracoNode"))));

                tr.Complete();
            }
        }

        public override void Down()
        {
            // not implemented
        }

        private void InsertLockObject(int id, string uniqueId, string text)
        {
            var exists = Context.Database.Exists<NodeDto>(id);
            if (exists) return;

            Context.Database.Insert("umbracoNode", "id", false, new NodeDto
            {
                NodeId = id,
                Trashed = false,
                ParentId = -1,
                UserId = 0,
                Level = 1,
                Path = "-1," + id,
                SortOrder = 0,
                UniqueId = new Guid(uniqueId),
                Text = text,
                NodeObjectType = new Guid(Constants.ObjectTypes.LockObject),
                CreateDate = DateTime.Now
            });
        }
    }
}
