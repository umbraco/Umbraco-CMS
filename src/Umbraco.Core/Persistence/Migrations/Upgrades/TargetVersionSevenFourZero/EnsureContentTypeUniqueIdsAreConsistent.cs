using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
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
            var docTypeGuid = new Guid(Constants.ObjectTypes.DocumentType);
            var mediaTypeGuid = new Guid(Constants.ObjectTypes.MediaType);
            var memberTypeGuid = new Guid(Constants.ObjectTypes.MemberType);

            var sql = new Sql()
                .Select("umbracoNode.id,cmsContentType.alias,umbracoNode.nodeObjectType")
                .From<NodeDto>(SqlSyntax)
                .InnerJoin<ContentTypeDto>(SqlSyntax)
                .On<NodeDto, ContentTypeDto>(SqlSyntax, dto => dto.NodeId, dto => dto.NodeId);

            var rows = Context.Database.Fetch<dynamic>(sql)
                .Where(x => x.nodeObjectType == docTypeGuid || x.nodeObjectType == mediaTypeGuid || x.nodeObjectType == memberTypeGuid);
            foreach (var row in rows)
            {
                // casting to string to gain access to ToGuid extension method.
                var alias = (string)row.alias.ToString();
                var nodeType = ((Guid) row.nodeObjectType).ToString();
                var consistentGuid = (alias + nodeType).ToGuid();
                Update.Table("umbracoNode").Set(new { uniqueID = consistentGuid }).Where(new { id = row.id });
            }
        }

        public override void Down()
        {
        }
    }
}