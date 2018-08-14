using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class RefactorXmlColumns : MigrationBase
    {
        public RefactorXmlColumns(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            if (ColumnExists("cmsContentXml", "Rv") == false)
                Alter.Table("cmsContentXml").AddColumn("Rv").AsInt64().NotNullable().WithDefaultValue(0).Do();

            if (ColumnExists("cmsPreviewXml", "Rv") == false)
                Alter.Table("cmsPreviewXml").AddColumn("Rv").AsInt64().NotNullable().WithDefaultValue(0).Do();

            // remove the any PK_ and the FK_ to cmsContentVersion.VersionId
            if (DatabaseType.IsMySql())
            {
                Delete.PrimaryKey("PK_cmsPreviewXml").FromTable("cmsPreviewXml").Do();

                Delete.ForeignKey().FromTable("cmsPreviewXml").ForeignColumn("VersionId")
                    .ToTable("cmsContentVersion").PrimaryColumn("VersionId")
                    .Do();
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
                        Logger.Warn<RefactorXmlColumns>("Duplicate constraint '{Constraint}'", c.Item3);
                        continue;
                    }
                    dups.Add(keyName);
                    Delete.PrimaryKey(c.Item3).FromTable(c.Item1).Do();
                }
                foreach (var c in constraints.Where(x => x.Item1.InvariantEquals("cmsPreviewXml") && x.Item3.InvariantStartsWith("FK_cmsPreviewXml_cmsContentVersion")))
                {
                    Delete.ForeignKey().FromTable("cmsPreviewXml").ForeignColumn("VersionId")
                        .ToTable("cmsContentVersion").PrimaryColumn("VersionId")
                        .Do();
                }
            }

            if (ColumnExists("cmsPreviewXml", "Timestamp"))
                Delete.Column("Timestamp").FromTable("cmsPreviewXml").Do();

            if (ColumnExists("cmsPreviewXml", "VersionId"))
            {
                RemoveDuplicates();
                Delete.Column("VersionId").FromTable("cmsPreviewXml").Do();
            }

            // re-create the primary key
            Create.PrimaryKey("PK_cmsPreviewXml")
                .OnTable("cmsPreviewXml")
                .Columns(new[] { "nodeId" })
                .Do();
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
