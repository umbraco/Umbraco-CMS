using System.Linq;
using AutoMapper;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenTwoZero
{
    

    [Migration("7.2.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class AlterDataTypePreValueTable : MigrationBase
    {
        public AlterDataTypePreValueTable(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).Distinct().ToArray();

            //Check if it's already text
            if (columns.Any(x => x.ColumnName.InvariantEquals("value") && x.TableName.InvariantEquals("cmsDataTypePreValues") 
                //mysql check
                && (x.DataType.InvariantEquals("longtext") == false 
                    //sql server check
                    && x.DataType.InvariantEquals("ntext") == false)))
            {
                //To text
                var textType = SqlSyntax.GetSpecialDbType(SpecialDbTypes.NTEXT);
                Alter.Table("cmsDataTypePreValues").AlterColumn("value").AsCustom(textType).Nullable();
            }
            
        }

        public override void Down()
        {
            //back to 2500 chars
            Alter.Table("cmsDataTypePreValues").AlterColumn("value").AsString(2500).Nullable();
        }
    }
}