using System;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class ContentVersionCultureVariationDto
    {
        private const string TableName = Constants.DatabaseSchema.Tables.ContentVersionCultureVariation;

        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("versionId")]
        [ForeignKey(typeof(ContentVersionDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_VersionId", ForColumns = "versionId,languageId")]
        public int VersionId { get; set; }

        [Column("languageId")]
        [ForeignKey(typeof(LanguageDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
        public int LanguageId { get; set; }

        [Column("name")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Name { get; set; }

        [Column("available")]
        public bool Available { get; set; }

        [Column("availableDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? AvailableDate { get; set; }

        [Column("availableUserId")]
        // [ForeignKey(typeof(UserDto))] -- there is no foreign key so we can delete users without deleting associated content
        //[NullSetting(NullSetting = NullSettings.Null)]
        public int AvailableUserId { get; set; }

        [Column("edited")]
        public bool Edited { get; set; }
    }
}
