using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenTwoZero
{
    [Migration("7.2.0", 3, GlobalSettings.UmbracoMigrationName)]
    public class AddIndexToUmbracoNodeTable : MigrationBase
    {
        private readonly bool _skipIndexCheck;

        internal AddIndexToUmbracoNodeTable(ISqlSyntaxProvider sqlSyntax, ILogger logger, bool skipIndexCheck) : base(sqlSyntax, logger)
        {
            _skipIndexCheck = skipIndexCheck;
        }

        public AddIndexToUmbracoNodeTable(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var dbIndexes = _skipIndexCheck ? new DbIndexDefinition[] { } : SqlSyntax.GetDefinedIndexes(Context.Database)
                .Select(x => new DbIndexDefinition
                {
                    TableName = x.Item1,
                    IndexName = x.Item2,
                    ColumnName = x.Item3,
                    IsUnique = x.Item4
                }).ToArray();

            //make sure it doesn't already exist
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoNodeUniqueID")) == false)
            {
				Create.Index("IX_umbracoNodeUniqueID").OnTable("umbracoNode").OnColumn("uniqueID").Ascending().WithOptions().NonClustered();
            }
        }

        public override void Down()
        {
            Delete.Index("IX_umbracoNodeUniqueID").OnTable("umbracoNode");
        }
    }
}