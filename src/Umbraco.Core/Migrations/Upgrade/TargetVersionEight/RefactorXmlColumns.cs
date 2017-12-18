using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Migrations.Upgrade.TargetVersionEight
{
    [Migration("8.0.0", 100, Constants.System.UmbracoMigrationName)]
    public class RefactorXmlColumns : MigrationBase
    {
        public RefactorXmlColumns(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            if (ColumnExists("cmsContentXml", "Rv") == false)
                Alter.Table("cmsContentXml").AddColumn("Rv").AsInt64().NotNullable().WithDefaultValue(0);

            if (ColumnExists("cmsPreviewXml", "Rv") == false)
                Alter.Table("cmsPreviewXml").AddColumn("Rv").AsInt64().NotNullable().WithDefaultValue(0);

            // remove the any PK_ and the FK_ to cmsContentVersion.VersionId
            if (DatabaseType.IsMySql())
            {
                Delete.PrimaryKey("PK_cmsPreviewXml").FromTable("cmsPreviewXml");

                Delete.ForeignKey().FromTable("cmsPreviewXml").ForeignColumn("VersionId")
                    .ToTable("cmsContentVersion").PrimaryColumn("VersionId");
            }
            else
            {
                var constraints = SqlSyntax.GetConstraintsPerColumn(Context.Database).Distinct().ToArray();
                var dups = new List<string>();
                foreach (var c in constraints.Where(x => x.Item1.InvariantEquals("cmsPreviewXml") && x.Item3.InvariantStartsWith("PK_")))
                {
                    var keyName = c.Item3.ToLowerInvariant();
                    if (dups.Contains(keyName))
                    {
                        Logger.Warn<RefactorXmlColumns>("Duplicate constraint " + c.Item3);
                        continue;
                    }
                    dups.Add(keyName);
                    Delete.PrimaryKey(c.Item3).FromTable(c.Item1);
                }
                foreach (var c in constraints.Where(x => x.Item1.InvariantEquals("cmsPreviewXml") && x.Item3.InvariantStartsWith("FK_cmsPreviewXml_cmsContentVersion")))
                {
                    Delete.ForeignKey().FromTable("cmsPreviewXml").ForeignColumn("VersionId")
                        .ToTable("cmsContentVersion").PrimaryColumn("VersionId");
                }
            }

            if (ColumnExists("cmsPreviewXml", "Timestamp"))
                Delete.Column("Timestamp").FromTable("cmsPreviewXml");

            if (ColumnExists("cmsPreviewXml", "VersionId"))
            {
                RemoveDuplicates();
                Delete.Column("VersionId").FromTable("cmsPreviewXml");
            }

            // re-create the primary key
            Create.PrimaryKey("PK_cmsPreviewXml")
                .OnTable("cmsPreviewXml")
                .Columns(new[] { "nodeId" });
        }

        public override void Down()
        {
            throw new DataLossException("Downgrading is not supported.");

            //if (Exists("cmsContentXml", "Rv"))
            //    Delete.Column("Rv").FromTable("cmsContentXml");
            //if (Exists("cmsPreviewXml", "Rv"))
            //    Delete.Column("Rv").FromTable("cmsContentXml");
        }

        private bool ColumnExists(string tableName, string columnName)
        {
            // that's ok even on MySql
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).Distinct().ToArray();
            return columns.Any(x => x.TableName.InvariantEquals(tableName) && x.ColumnName.InvariantEquals(columnName));
        }

        private void RemoveDuplicates()
        {
            const string sql = @"delete from cmsPreviewXml where versionId in (
select cmsPreviewXml.versionId from cmsPreviewXml
join cmsDocument on cmsPreviewXml.versionId=cmsDocument.versionId
where cmsDocument.newest <> 1)";

            Context.Database.Execute(sql);
        }
    }
}
