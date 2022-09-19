using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_3_0
{
    public class AddComputedNodeColumns : MigrationBase
    {
        public AddComputedNodeColumns(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (Context.Database.DatabaseType == NPoco.DatabaseType.SQLite)
            {
                //SQLCE
                if (ColumnExists(NodeDto.TableName, "textUpper") is false)
                {
                    var addTextUpperComputedColumnSql = $"ALTER TABLE {NodeDto.TableName} ADD textUpper TEXT GENERATED ALWAYS AS (upper(text)) STORED";
                    Database.Execute(addTextUpperComputedColumnSql);
                }
                if (ColumnExists(NodeDto.TableName, "pathUpper") is false)
                {
                    var addPathUpperComputedColumnSql = $"ALTER TABLE {NodeDto.TableName} ADD pathUpper TEXT GENERATED ALWAYS AS (upper(path)) STORED";
                    Database.Execute(addPathUpperComputedColumnSql);
                }
            }
            else
            {
                //SQL Server
                if (ColumnExists(NodeDto.TableName, "textUpper") is false)
                {
                    var addTextUpperComputedColumnSql = $"ALTER TABLE {NodeDto.TableName} ADD textUpper AS UPPER(text)";
                    Database.Execute(addTextUpperComputedColumnSql);
                }
                if (ColumnExists(NodeDto.TableName, "pathUpper") is false)
                {
                    var addPathUpperComputedColumnSql = $"ALTER TABLE {NodeDto.TableName} ADD pathUpper AS UPPER(path)";
                    Database.Execute(addPathUpperComputedColumnSql);
                }
            }

            //Superceeded by _ObjectType_trashed_sorted
            var nodeDtoObjectTypeIndex = $"IX_{NodeDto.TableName}_ObjectType";
            DeleteIndex<NodeDto>(nodeDtoObjectTypeIndex);

            var nodeDtoObjectTypeTextUpperIndex = $"IX_{NodeDto.TableName}_ObjectType_textUpper";
            DeleteIndex<NodeDto>(nodeDtoObjectTypeTextUpperIndex);
            CreateIndex<NodeDto>(nodeDtoObjectTypeTextUpperIndex);
        }
    }
}
