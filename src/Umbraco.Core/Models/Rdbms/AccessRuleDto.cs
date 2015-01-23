using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoAccessRule")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class AccessRuleDto
    {
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_umbracoAccessRule")]
        public int Id { get; set; }

        [Column("accessId")]
        [ForeignKey(typeof(AccessDto), Name = "FK_umbracoAccessRule_umbracoAccess_id")]
        public int AccessId { get; set; }

        [Column("claim")]       
        public string Claim { get; set; }

        [Column("claimType")]
        public string ClaimType { get; set; }
    }
}