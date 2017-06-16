﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 1, Constants.System.UmbracoMigrationName)]
    public class CreateCacheInstructionTable : MigrationBase
    {
        public CreateCacheInstructionTable(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {

            //Don't exeucte if the table is already there
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("umbracoCacheInstruction")) return;

            var textType = SqlSyntax.GetSpecialDbType(SpecialDbTypes.NTEXT);

            Create.Table("umbracoCacheInstruction")
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey("PK_umbracoCacheInstruction").Identity()
                .WithColumn("utcStamp").AsDateTime().NotNullable()
                .WithColumn("jsonInstruction").AsCustom(textType).NotNullable()
                .WithColumn("originated").AsString(500).NotNullable();
            
        }

        public override void Down()
        {
            Delete.PrimaryKey("PK_umbracoCacheInstruction").FromTable("cmsContentType2ContentType");
            Delete.Table("cmsContentType2ContentType");
        }
    }
}
