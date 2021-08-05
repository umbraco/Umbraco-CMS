using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos
{
    [TableName(TableName)]
    [ExplicitColumns]
    [PrimaryKey("Id")]
    internal class TestDto
    {
        public const string TableName = Cms.Core.Constants.DatabaseSchema.Tables.Test;

        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("name")]
        [Length(255)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Name { get; set; }
    }
}
