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
            Delete.FromTable("umbracoMigration").AllRows();
            var migrations = Context.Database.Fetch<MigrationDto>(new Sql().Select("*").From<MigrationDto>(SqlSyntax));
            foreach (var migration in migrations)
            {
                Insert.IntoTable("umbracoMigration")
                    .Row(new {name = migration.Name, createDate = migration.CreateDate, version = migration.Version});
            }
        }

        public override void Down()
        {
        }
    }
}