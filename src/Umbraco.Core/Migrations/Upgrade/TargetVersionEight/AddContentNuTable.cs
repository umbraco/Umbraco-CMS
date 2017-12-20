using System.Data;
using System.Linq;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Migrations.Upgrade.TargetVersionEight
{
    [Migration("8.0.0", 100, Constants.System.UmbracoMigrationName)]
    class AddContentNuTable : MigrationBase
    {
        public AddContentNuTable(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("cmsContentNu")) return;

            var textType = SqlSyntax.GetSpecialDbType(SpecialDbTypes.NTEXT);

            Create.Table("cmsContentNu")
                .WithColumn("nodeId").AsInt32().NotNullable()
                .WithColumn("published").AsBoolean().NotNullable()
                .WithColumn("data").AsCustom(textType).NotNullable()
                .WithColumn("rv").AsInt64().NotNullable().WithDefaultValue(0)
                .Do();

            Create.PrimaryKey("PK_cmsContentNu")
                .OnTable("cmsContentNu")
                .Columns(new[] { "nodeId", "published" })
                .Do();

            Create.ForeignKey("FK_cmsContentNu_umbracoNode_id")
                .FromTable("cmsContentNu")
                .ForeignColumn("nodeId")
                .ToTable("umbracoNode")
                .PrimaryColumn("id")
                .OnDelete(Rule.Cascade)
                .OnUpdate(Rule.None)
                .Do();
        }

        public override void Down()
        { }
    }
}
