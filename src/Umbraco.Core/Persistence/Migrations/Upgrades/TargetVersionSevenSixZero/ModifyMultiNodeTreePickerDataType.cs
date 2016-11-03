using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 1, GlobalSettings.UmbracoMigrationName)]
    public class ModifyMultiNodeTreePickerDataType : MigrationBase
    {
        public ModifyMultiNodeTreePickerDataType(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {

        }

        public override void Down()
        {
            // dont be silly
        }

        public override void Up()
        {
            // get all data
            var sql = string.Format(@"
             SELECT    pd.id, pd.dataNvarchar from cmsPropertyData pd
             JOIN      cmsPropertyType pt on pd.propertyTypeId = pt.id
             JOIN      cmsDataType dt on dt.nodeid = pt.dataTypeId
             WHERE     dt.propertyEditorAlias in ('{0}','{1}')
             AND       dt.dbType = 'Nvarchar'", Constants.PropertyEditors.MultiNodeTreePickerAlias, Constants.PropertyEditors.MultipleMediaPickerAlias);

            var multiNodeRelations = Context.Database.Fetch<PropertyDataDto>(sql);

            // I'm not deleting the old value on purpose to prevent deleting data when running the upgrade multiple times.

            foreach (var propertyData in multiNodeRelations)
            {
                Update.Table("cmsPropertyData")
                    .Set(new
                    {
                        dataNtext = propertyData.VarChar
                    })
                    .Where(new {Id = propertyData.Id});
            }

            Update.Table("cmsDataType")
                .Set(new
                {
                    dbType = "Ntext"
                })
                .Where(new
                {
                    propertyEditorAlias = Constants.PropertyEditors.MultiNodeTreePickerAlias,
                    dbTYpe = "Nvarchar"
                });

            Update.Table("cmsDataType")
                .Set(new
                {
                    dbType = "Ntext"
                })
                .Where(new
                {
                    propertyEditorAlias = Constants.PropertyEditors.MultipleMediaPickerAlias,
                    dbTYpe = "Nvarchar"
                });
        }
    }
}
