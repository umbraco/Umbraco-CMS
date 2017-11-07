using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName(TableName)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class DocumentVersionDto
    {
        private const string TableName = Constants.DatabaseSchema.Tables.DocumentVersion;

        [Column("id")]
        [PrimaryKeyColumn]
        [ForeignKey(typeof(ContentVersionDto))]
        public int Id { get; set; }

        [Column("templateId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [ForeignKey(typeof(TemplateDto), Column = "nodeId")]
        public int? TemplateId { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ReferenceMemberName = "VersionId")]
        public ContentVersionDto ContentVersionDto { get; set; }
    }
}
