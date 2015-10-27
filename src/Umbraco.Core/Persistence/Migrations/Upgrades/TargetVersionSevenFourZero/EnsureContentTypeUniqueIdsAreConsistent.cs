using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFourZero
{
    /// <summary>
    /// Courier on v. 7.4+ will handle ContentTypes using GUIDs instead of
    /// alias, so we need to ensure that these are initially consistent on
    /// all environments (based on the alias).
    /// </summary>
    [Migration("7.4.0", 2, GlobalSettings.UmbracoMigrationName)]
    public class EnsureContentTypeUniqueIdsAreConsistent : MigrationBase
    {
        public EnsureContentTypeUniqueIdsAreConsistent(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }
        
        public override void Up()
        {
            var sql = @"select umbracoNode.id,
	                      cmsContentType.alias
                      from umbracoNode
                      inner join cmsContentType
                      on umbracoNode.id = cmsContentType.nodeId
                      where nodeObjectType = '" + Constants.ObjectTypes.DocumentType + "'";
            var rows = Context.Database.Fetch<dynamic>(sql);
            foreach (var row in rows)
            {
                // casting to string to gain access to ToGuid extension method.
                var alias = (string)row.alias.ToString();
                var consistentGuid = (alias + Constants.ObjectTypes.DocumentType).ToGuid();
                Update.Table("umbracoNode").Set(new { uniqueID = consistentGuid }).Where(new { id = row.id });
            }
        }

        public override void Down()
        {
        }
    }
}