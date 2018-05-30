using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class DocumentCultureVariationDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.DocumentCultureVariation;

        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_NodeId", ForColumns = "nodeId,languageId")]
        public int NodeId { get; set; }

        [Column("languageId")]
        [ForeignKey(typeof(LanguageDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
        public int LanguageId { get; set; }

        // this is convenient to carry the culture around, but has no db counterpart
        [Ignore]
        public string Culture { get; set; }

        [Column("edited")]
        public bool Edited { get; set; }
    }
}
