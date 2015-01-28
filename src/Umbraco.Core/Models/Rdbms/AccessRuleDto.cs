using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoAccessRule")]
    [PrimaryKey("id", autoIncrement = false)]
    [ExplicitColumns]
    internal class AccessRuleDto
    {
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_umbracoAccessRule", AutoIncrement = false)]
        public Guid Id { get; set; }

        [Column("accessId")]
        [ForeignKey(typeof(AccessDto), Name = "FK_umbracoAccessRule_umbracoAccess_id")]
        public Guid AccessId { get; set; }

        [Column("ruleValue")]
        [Index(IndexTypes.UniqueNonClustered, ForColumns = "ruleValue,ruleType,accessId", Name = "IX_umbracoAccessRule")]
        public string RuleValue { get; set; }

        [Column("ruleType")]
        public string RuleType { get; set; }

        [Column("createDate")]
        [Constraint(Default = "getdate()")]
        public DateTime CreateDate { get; set; }

        [Column("updateDate")]
        [Constraint(Default = "getdate()")]
        public DateTime UpdateDate { get; set; }
    }
}