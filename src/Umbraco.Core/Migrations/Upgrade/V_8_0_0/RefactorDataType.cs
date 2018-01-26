using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NPoco;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class RefactorDataType : MigrationBase
    {
        public RefactorDataType(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // delete *all* keys and indexes - because of FKs
            Delete.KeysAndIndexes().Do();

            // drop and create columns
            Delete.Column("pk").FromTable("cmsDataType").Do();

            // rename the table
            Rename.Table("cmsDataType").To(Constants.DatabaseSchema.Tables.DataType).Do();

            // create column
            // fixme it is annoying that these are NOT written out to the log?!
            AddColumn<DataTypeDto>(Constants.DatabaseSchema.Tables.DataType, "config");
            Database.Execute(Sql().Update<DataTypeDto>(u => u.Set(x => x.Configuration, string.Empty)));

            // re-create *all* keys and indexes
            foreach (var x in DatabaseSchemaCreator.OrderedTables)
                Create.KeysAndIndexes(x.Value).Do();

            var sql = Sql()
                .Select<DataTypeDto>()
                .AndSelect<PreValueDto>(x => x.Alias, x => x.SortOrder, x => x.Value)
                .From<DataTypeDto>()
                .InnerJoin<PreValueDto>().On<DataTypeDto, PreValueDto>((left, right) => left.NodeId == right.NodeId)
                .OrderBy<DataTypeDto>(x => x.NodeId)
                .AndBy<PreValueDto>(x => x.SortOrder);

            var dtos = Database.Fetch<PreValueDto>(sql).GroupBy(x => x.NodeId);

            foreach (var group in dtos)
            {
                var dataType = Database.Fetch<DataTypeDto>(Sql()
                    .Select<DataTypeDto>()
                    .From<DataTypeDto>()
                    .Where<DataTypeDto>(x => x.NodeId == group.Key)).First();

                var aliases = group.Select(x => x.Alias).Distinct().ToArray();
                if (aliases.Length == 1 && string.IsNullOrWhiteSpace(aliases[0]))
                {
                    // array-based prevalues
                    var values = new Dictionary<string, object> { ["values"] = group.OrderBy(x => x.SortOrder).Select(x => x.Value).ToArray() };
                    dataType.Configuration = JsonConvert.SerializeObject(values);
                }
                else
                {
                    // fixme deal with null or empty aliases
                    // fixme deal with duplicate aliases
                    // in these cases, fallback to array?

                    // dictionary-base prevalues
                    var values = group.ToDictionary(x => x.Alias, x => x.Value);
                    dataType.Configuration = JsonConvert.SerializeObject(values);
                }

                Database.Update(dataType);
            }

            // drop preValues table
            // FIXME keep it around for now
            //Delete.Table("cmsDataTypePreValues");
        }

        [TableName("cmsDataTypePreValues")]
        [ExplicitColumns]
        public class PreValueDto
        {
            [Column("datatypeNodeId")]
            public int NodeId { get; set; }

            [Column("alias")]
            public string Alias { get; set; }

            [Column("sortorder")]
            public int SortOrder { get; set; }

            [Column("value")]
            public string Value { get; set; }
        }
    }
}
