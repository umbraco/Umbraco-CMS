using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class AccessRuleDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.AccessRule;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    internal const string AccessIdColumnName = "accessId";

    private const string RuleValueColumnName = "ruleValue";
    private const string RuleTypeColumnName = "ruleType";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_umbracoAccessRule", AutoIncrement = false)]
    public Guid Id { get; set; }

    [Column(AccessIdColumnName)]
    [ForeignKey(typeof(AccessDto), Name = "FK_umbracoAccessRule_umbracoAccess_id")]
    public Guid AccessId { get; set; }

    [Column(RuleValueColumnName)]
    [Index(IndexTypes.UniqueNonClustered, ForColumns = $"{RuleValueColumnName},{RuleTypeColumnName},{AccessIdColumnName}", Name = "IX_umbracoAccessRule")]
    public string? RuleValue { get; set; }

    [Column(RuleTypeColumnName)]
    public string? RuleType { get; set; }

    [Column("createDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime CreateDate { get; set; }

    [Column("updateDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime UpdateDate { get; set; }
}
