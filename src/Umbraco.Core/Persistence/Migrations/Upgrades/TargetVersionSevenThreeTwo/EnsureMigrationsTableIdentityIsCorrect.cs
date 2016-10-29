using System.Collections.Generic;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeTwo
{
    /// <summary>
    /// This reinserts all migrations in the migrations table to account for initial rows inserted
    /// on creation without identity enabled.
    /// </summary>
    [Migration("7.3.2", 0, GlobalSettings.UmbracoMigrationName)]
    public class EnsureMigrationsTableIdentityIsCorrect : MigrationBase
    {
        public EnsureMigrationsTableIdentityIsCorrect(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            // Due to the delayed execution of migrations, we have to wrap this code in Execute.Code to ensure the previous
            // migration steps (esp. creating the migrations table) have completed before trying to fetch migrations from
            // this table.
            List<MigrationDto> migrations = null;
            Execute.Code(db =>
            {
                migrations = Context.Database.Fetch<MigrationDto>(new Sql().Select("*").From<MigrationDto>(SqlSyntax));
                return string.Empty;
            });
            Delete.FromTable("umbracoMigration").AllRows();
            Execute.Code(database =>
            {
                if (migrations != null)
                {
                    foreach (var migration in migrations)
                    {
                        database.Insert("umbracoMigration", "id", true,
                            new {name = migration.Name, createDate = migration.CreateDate, version = migration.Version});
                    }
                }
                return string.Empty;
            });
        }

        public override void Down()
        {
        }
    }
}