using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_17_0
{
    public class UpgradeNtextColumns : MigrationBase
    {
        public UpgradeNtextColumns(IMigrationContext context)
           : base(context)
        { }
        public override void Migrate()
        {
            //Change ntext columns to nvarchar(max)
            //cmsContentNu.data
            //umbracoPropertyData.textValue
            if (DatabaseType.IsSqlCe())
            {
                //No Support for nvarchar(max) in SQLCE
                return;
            }
            AlterColumn<CacheInstructionDto>(Constants.DatabaseSchema.Tables.CacheInstruction, "jsonInstruction");
            AlterColumn<ContentNuDto>(Constants.DatabaseSchema.Tables.NodeData, "data");
            AlterColumn<DataTypeDto>(Constants.DatabaseSchema.Tables.DataType, "config");
            AlterColumn<ExternalLoginDto>(Constants.DatabaseSchema.Tables.ExternalLogin, "userData");
            AlterColumn<PropertyDataDto>(Constants.DatabaseSchema.Tables.PropertyData, "textValue");
            AlterColumn<UserDto>(Constants.DatabaseSchema.Tables.User, "tourData");
        }

    }
}
