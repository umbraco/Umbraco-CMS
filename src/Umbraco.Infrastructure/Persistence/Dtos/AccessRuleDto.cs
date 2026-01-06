using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class AccessRuleDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.AccessRule;
    public const string PrimaryKeyName = Constants.DatabaseSchema.PrimaryKeyNameId;

    [Column(PrimaryKeyName)]
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

    [Column("createDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime CreateDate { get; set; }

    [Column("updateDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime UpdateDate { get; set; }
}
