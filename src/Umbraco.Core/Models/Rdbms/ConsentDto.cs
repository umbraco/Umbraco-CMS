using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName(TableName)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    public class ConsentDto
    {
        internal const string TableName = "umbracoConsent";

        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("source")]
        [Length(512)] // aligned with Deploy SignatureDto
        [Index(IndexTypes.UniqueNonClustered, ForColumns = "source, actionType, action", Name = "IX_" + TableName + "_UniqueSourceAction")]
        public string Source { get; set; }

        [Column("action")]
        [Length(512)] // aligned with Deploy SignatureDto
        public string Action { get; set; }

        [Column("actionType")]
        [Length(128)]
        public string ActionType { get; set; }

        [Column("updateDate")]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime UpdateDate { get; set; }

        [Column("state")]
        public int State { get; set; }

        [Column("comment")]
        public string Comment { get; set; }
    }
}
