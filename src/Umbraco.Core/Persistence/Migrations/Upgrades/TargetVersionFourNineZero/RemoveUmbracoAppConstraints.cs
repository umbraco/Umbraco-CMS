using System.Data;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionFourNineZero
{
    [MigrationAttribute("4.9.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class RemoveUmbracoAppConstraints : MigrationBase
    {
        public RemoveUmbracoAppConstraints(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //This will work on mysql and should work on mssql however the old keys were not named consistently with how the keys are 
            // structured now. So we need to do a check and manually remove them based on their old aliases.

            if (this.Context.CurrentDatabaseProvider == DatabaseProviders.MySql)
            {
                Delete.ForeignKey().FromTable("umbracoUser2app").ForeignColumn("app").ToTable("umbracoApp").PrimaryColumn("appAlias");
                Delete.ForeignKey().FromTable("umbracoAppTree").ForeignColumn("appAlias").ToTable("umbracoApp").PrimaryColumn("appAlias");
            }
            else
            {
                //These are the old aliases, before removing them, check they exist
                var constraints = SqlSyntax.GetConstraintsPerColumn(Context.Database).Distinct().ToArray();

                if (constraints.Any(x => x.Item1.InvariantEquals("umbracoUser2app") && x.Item3.InvariantEquals("FK_umbracoUser2app_umbracoApp")))
                {
                    Delete.ForeignKey("FK_umbracoUser2app_umbracoApp").OnTable("umbracoUser2app");
                    //name this migration, this is a hack for DeleteAppTables to ensure it's not executed twice
                    ((MigrationExpressionBase) Context.Expressions.Last()).Name = "FK_umbracoUser2app_umbracoApp";
                }
                if (constraints.Any(x => x.Item1.InvariantEquals("umbracoUser2app") && x.Item3.InvariantEquals("FK_umbracoUser2app_umbracoUser")))
                {
                    Delete.ForeignKey("FK_umbracoUser2app_umbracoUser").OnTable("umbracoUser2app");
                }
                
            }
            
        }

        public override void Down()
        {
            Create.ForeignKey("FK_umbracoUser2app_umbracoApp").FromTable("umbracoUser2app").ForeignColumn("app")
                .ToTable("umbracoApp").PrimaryColumn("appAlias").OnDeleteOrUpdate(Rule.None);

            Create.ForeignKey("FK_umbracoAppTree_umbracoApp").FromTable("umbracoAppTree").ForeignColumn("appAlias")
                .ToTable("umbracoApp").PrimaryColumn("appAlias").OnDeleteOrUpdate(Rule.None);
        }
    }
}