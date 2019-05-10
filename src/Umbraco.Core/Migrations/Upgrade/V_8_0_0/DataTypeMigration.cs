using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NPoco;
using Umbraco.Core.Composing;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{

    public class DataTypeMigration : MigrationBase
    {
        public DataTypeMigration(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // drop and create columns
            Delete.Column("pk").FromTable("cmsDataType").Do();

            // rename the table
            Rename.Table("cmsDataType").To(Constants.DatabaseSchema.Tables.DataType).Do();

            // create column
            AddColumn<DataTypeDto>(Constants.DatabaseSchema.Tables.DataType, "config");
            Execute.Sql(Sql().Update<DataTypeDto>(u => u.Set(x => x.Configuration, string.Empty))).Do();

            // renames
            Execute.Sql(Sql()
                .Update<DataTypeDto>(u => u.Set(x => x.EditorAlias, "Umbraco.ColorPicker"))
                .Where<DataTypeDto>(x => x.EditorAlias == "Umbraco.ColorPickerAlias")).Do();

            // from preValues to configuration...
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
                    // assuming we don't want to fall back to array
                    if (aliases.Length != group.Count() || aliases.Any(string.IsNullOrWhiteSpace))
                        throw new InvalidOperationException($"Cannot migrate datatype w/ id={dataType.NodeId} preValues: duplicate or null/empty alias.");

                    // must take care of all odd situations ;-(
                    object FmtPreValue(string alias, string preValue)
                    {
                        Current.Logger.Debug(typeof(DataTypeMigration), "DEBUG " + dataType.EditorAlias + " / " + alias);
                        if (dataType.EditorAlias == "Umbraco.MediaPicker2")
                        {
                            if (alias == "multiPicker" ||
                                alias == "onlyImages" ||
                                alias == "disableFolderSelect")
                                return preValue == "1";
                        }

                        return preValue.DetectIsJson() ? JsonConvert.DeserializeObject(preValue) : preValue;
                    }

                    // dictionary-base prevalues
                    var values = group.ToDictionary(x => x.Alias, x => FmtPreValue(x.Alias, x.Value));
                    dataType.Configuration = JsonConvert.SerializeObject(values);
                }

                Database.Update(dataType);
            }
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
