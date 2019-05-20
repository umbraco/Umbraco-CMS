using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using static Umbraco.Core.Persistence.SqlExtensionsStatics;

namespace Umbraco.Core.Migrations.Upgrade.V_7_8_0
{
    internal class AddMediaVersionTable : MigrationBase
    {
        public AddMediaVersionTable(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();

            if (tables.InvariantContains(Constants.DatabaseSchema.Tables.MediaVersion)) return;

            Create.Table<MediaVersionDto>().Do();
            MigrateMediaPaths();
        }

        private void MigrateMediaPaths()
        {
            // this may not be the most efficient way to do it, compared to how it's done in v7, but this
            // migration should only run for v8 sites that are being developed, before v8 is released, so
            // no big sites and performances don't matter here - keep it simple

            var sql = Sql()
                .Select<PropertyDataDto>(x => x.VarcharValue, x => x.TextValue)
                    .AndSelect<ContentVersionDto>(x => Alias(x.Id, "versionId"))
                .From<PropertyDataDto>()
                    .InnerJoin<PropertyTypeDto>().On<PropertyDataDto, PropertyTypeDto>((left, right) => left.PropertyTypeId == right.Id)
                    .InnerJoin<ContentVersionDto>().On<PropertyDataDto, ContentVersionDto>((left, right) => left.VersionId == right.Id)
                    .InnerJoin<NodeDto>().On<ContentVersionDto, NodeDto>((left, right) => left.NodeId == right.NodeId)
                .Where<PropertyTypeDto>(x => x.Alias == "umbracoFile")
                .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Media);

                var paths = new List<MediaVersionDto>();

                //using QUERY = a db cursor, we won't load this all into memory first, just row by row
                foreach (var row in Database.Query<dynamic>(sql))
                {
                    // if there's values then ensure there's a media path match and extract it
                    string mediaPath = null;
                    if (
                        (row.varcharValue != null && ContentBaseFactory.TryMatch((string) row.varcharValue, out mediaPath))
                        || (row.textValue != null && ContentBaseFactory.TryMatch((string) row.textValue, out mediaPath)))
                    {
                        paths.Add(new MediaVersionDto
                        {
                            Id = (int) row.versionId,
                            Path = mediaPath
                        });
                    }
                }

                // bulk insert
                Database.BulkInsertRecords(paths);
        }
    }
}
