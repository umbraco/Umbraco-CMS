using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFifteenFive
{
    public class UpdateJsonWithMinifiedJson : MigrationBase
    {
        public UpdateJsonWithMinifiedJson(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            // Find all JSON characters in both dataNvarchar & dataNtext
            // deserialize each one, reserialize with Formatting.None
            var allRecords = Context.Database.ExecuteScalar<IEnumerable<TempDto>>(@"SELECT pd.id, pd.dataNvarchar, pd.dataNtext FROM cmsPropertyData pd " +
                                                                                  "WHERE (pd.dataNvarchar IS NOT NULL AND pd.dataNText IS NOT NULL)" +
                                                                                  "AND (pd.dataNvarchar != '' OR CAST(pd.dataNtext AS nvarchar(max)) != '')");

            var recordsToProcess = allRecords.Where(record => {
                return ((!string.IsNullOrEmpty(record.Nvarchar) && record.Nvarchar.DetectIsJson()) ||
                        (!string.IsNullOrEmpty(record.Text) && record.Text.DetectIsJson()));
            });

            foreach (var record in recordsToProcess)
            {
                // Deserialize/re-serialize JSON for Nvarchar
                if (!string.IsNullOrEmpty(record.Nvarchar) && record.Nvarchar.DetectIsJson())
                {
                    var result = JsonConvert.DeserializeObject(record.Nvarchar);
                    record.Nvarchar = JsonConvert.SerializeObject(result, Formatting.None);
                }

                // Deserialize/re-serialize JSON for Ntext
                if (!string.IsNullOrEmpty(record.Text) && record.Text.DetectIsJson())
                {
                    var result = JsonConvert.DeserializeObject(record.Text);
                    record.Text = JsonConvert.SerializeObject(result, Formatting.None);
                }

                Context.Database.Execute(@"UPDATE cmsPropertyData SET dataNvarchar = @0, dataNtext = @1 WHERE id = @2",
                                record.Nvarchar, record.Text, record.ID);

            }

        }

        public override void Down()
        {
        }

        internal class TempDto
        {
            [DataMember(Name = "id")]
            public int ID { get; set; }

            [DataMember(Name = "dataNvarchar")]
            public string Nvarchar { get; set; }

            [DataMember(Name = "dataNtext")]
            public string Text { get; set; }
        }
    }
}
