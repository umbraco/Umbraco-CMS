using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 1, GlobalSettings.UmbracoMigrationName)]
    public class CreateCacheInstructionTable : MigrationBase
    {
        public override void Up()
        {
            var textType = SqlSyntaxContext.SqlSyntaxProvider.GetSpecialDbType(SpecialDbTypes.NTEXT);

            Create.Table("umbracoCacheInstruction")
                .WithColumn("id").AsInt32().Identity().NotNullable()
                .WithColumn("utcStamp").AsDateTime().NotNullable()
                .WithColumn("jsonInstruction").AsCustom(textType).NotNullable();

            Create.PrimaryKey("PK_umbracoCacheInstruction")
                .OnTable("umbracoCacheInstruction")
                .Column("id");
        }

        public override void Down()
        {
            Delete.PrimaryKey("PK_umbracoCacheInstruction").FromTable("cmsContentType2ContentType");
            Delete.Table("cmsContentType2ContentType");
        }
    }
}
