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

            EnsureLockObject(Constants.System.ServersLock, "0AF5E610-A310-4B6F-925F-E928D5416AF7", "LOCK: Servers");
        }

        public override void Down()
        {
            // not implemented
        }

        private void EnsureLockObject(int id, string uniqueId, string text)
        {
            var exists = Context.Database.Exists<NodeDto>(id);
            if (exists) return;

            Insert
                .IntoTable("umbracoNode")
                .EnableIdentityInsert()
                .Row(new
                {
                    id = id, // NodeId
                    trashed = false,
                    parentId = -1,
                    nodeUser = 0,
                    level = 1,
                    path = "-1," + id,
                    sortOrder = 0,
                    uniqueId = new Guid(uniqueId),
                    text = text,
                    nodeObjectType = new Guid(Constants.ObjectTypes.LockObject),
                    createDate = DateTime.Now
                });
        }
    }
}
