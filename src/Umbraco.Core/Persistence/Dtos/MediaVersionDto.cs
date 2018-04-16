using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.MediaVersion)]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    internal class MediaVersionDto
    {
        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        [ForeignKey(typeof(ContentVersionDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + Constants.DatabaseSchema.Tables.MediaVersion, ForColumns = "id, path")]
        public int Id { get; set; }

        [Column("path")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Path { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne)]
        public ContentVersionDto ContentVersionDto { get; set; }
    }
}
