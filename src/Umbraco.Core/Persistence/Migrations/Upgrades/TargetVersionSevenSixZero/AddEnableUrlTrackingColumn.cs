using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    /// <summary>
    /// See: http://issues.umbraco.org/issue/U4-9540
    /// </summary>
    [Migration("7.6.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class AddEnableUrlTrackingColumn : MigrationBase
    {
        public AddEnableUrlTrackingColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            // Don't exeucte if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();
            if (columns.Any(x => x.TableName.InvariantEquals("cmsContentType") && x.ColumnName.InvariantEquals("enableUrlTracking")) == false)
            {
                Create.Column("enableUrlTracking").OnTable("cmsContentType").AsBoolean().NotNullable().WithDefaultValue(0);

                // Default to allow URL tracking for all content types that have a template (and hence a website page with a URL)
                Execute.Sql(@"UPDATE cmsContentType
                    SET enableUrlTracking = 1
                    FROM cmsContentType ct
                    INNER JOIN cmsDocumentType dt ON dt.contentTypeNodeId = ct.nodeId");
            }
        }

        public override void Down()
        {
        }
    }
}