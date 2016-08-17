using System;
using System.Linq;
using NPoco;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFourZero
{
    /// <summary>
    /// Courier on v. 7.4+ will handle ContentTypes using GUIDs instead of
    /// alias, so we need to ensure that these are initially consistent on
    /// all environments (based on the alias).
    /// </summary>
    [Migration("7.4.0", 3, GlobalSettings.UmbracoMigrationName)]
    public class EnsureContentTypeUniqueIdsAreConsistent : MigrationBase
    {
        public EnsureContentTypeUniqueIdsAreConsistent(IMigrationContext context)
            : base(context)
        { }
        
        public override void Up()
        {
            var objectTypes = new[]
            {
                Constants.ObjectTypes.DocumentTypeGuid,
                Constants.ObjectTypes.MediaTypeGuid,
                Constants.ObjectTypes.MemberTypeGuid,
            };

            var sql = Context.Database.Sql()
                .Select("umbracoNode.id,cmsContentType.alias,umbracoNode.nodeObjectType")
                .From<NodeDto>()
                .InnerJoin<ContentTypeDto>()
                .On<NodeDto, ContentTypeDto>(dto => dto.NodeId, dto => dto.NodeId)
                .WhereIn<NodeDto>(x => x.NodeObjectType, objectTypes);

            var rows = Context.Database.Fetch<dynamic>(sql);

            foreach (var row in rows)
            {
                // create a consistent guid from
                // alias + object type
                var guidSource = ((string) row.alias) + ((Guid) row.nodeObjectType);
                var guid = guidSource.ToGuid();

                // set the Unique Id to the one we've generated
                Update.Table("umbracoNode").Set(new { uniqueID = guid }).Where(new { id = row.id });
            }
        }

        public override void Down()
        { }
    }
}