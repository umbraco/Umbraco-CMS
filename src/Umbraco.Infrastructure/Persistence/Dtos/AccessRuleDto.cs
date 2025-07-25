using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.AccessRule)]
[PrimaryKey("id", AutoIncrement = false)]
[ExplicitColumns]
internal sealed class AccessRuleDto
{
    [Column("id")]
    [PrimaryKeyColumn(Name = "PK_umbracoAccessRule", AutoIncrement = false)]
    public Guid Id { get; set; }

    [Column("accessId")]
    [ForeignKey(typeof(AccessDto), Name = "FK_umbracoAccessRule_umbracoAccess_id")]
    public Guid AccessId { get; set; }

    [Column("ruleValue")]
    [Index(IndexTypes.UniqueNonClustered, ForColumns = "ruleValue,ruleType,accessId", Name = "IX_umbracoAccessRule")]
    public string? RuleValue { get; set; }

    [Column("ruleType")]
    public string? RuleType { get; set; }

    [Column("createDate", ForceToUtc = false)]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime CreateDate { get; set; }

    [Column("updateDate", ForceToUtc = false)]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime UpdateDate { get; set; }
}
