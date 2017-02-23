using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 0, Constants.System.UmbracoMigrationName)]
    public class AddRelationUniqueIdColumn : MigrationBase
    {
        public AddRelationUniqueIdColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoRelation") && x.ColumnName.InvariantEquals("uniqueId")) == false)
            {
                Create.Column("uniqueId").OnTable("umbracoRelation").AsGuid().Nullable();
                Execute.Code(UpdateRelationGuids);
                Alter.Table("umbracoRelation").AlterColumn("uniqueId").AsGuid().NotNullable();
                Create.Index("IX_umbracoRelation_UniqueId").OnTable("umbracoRelation").OnColumn("uniqueId")
                    .Ascending()
                    .WithOptions().NonClustered()
                    .WithOptions().Unique();
            }
        }

        private static string UpdateRelationGuids(Database database)
        {
            var relations = database.Query<dynamic>("SELECT id, parentId, childId, relType FROM umbracoRelation").ToList();

            var updates = new List<Tuple<int, Guid>>();

            foreach (var relation in relations)
            {
                var nodes = database.Fetch<NodeDto>("SELECT uniqueId FROM umbracoNode WHERE id = @parentId OR id = @childId", new {parentId = relation.parentId, childId = relation.childId});
                var relationType = database.Fetch<RelationTypeDto>("SELECT typeUniqueId FROM umbracoRelationType WHERE id = @relType", new {relType = relation.relType}).FirstOrDefault();
                if (nodes.Count != 2 || relationType == null)
                {
                    throw new Exception("Error while adding Unique IDs for relations.");
                }
                updates.Add(Tuple.Create(relation.id, GuidExtensions.Combine(nodes[0].UniqueId, nodes[1].UniqueId, relationType.UniqueId)));
            }

            foreach (var update in updates)
                database.Execute("UPDATE umbracoRelation set uniqueId=@guid WHERE id=@id", new {id = update.Item1, guid = update.Item2});

            return string.Empty;
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
