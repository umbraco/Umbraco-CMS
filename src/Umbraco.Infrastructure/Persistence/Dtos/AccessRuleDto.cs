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

    private const string AccessIdColumnName = "accessId";

    private const string RuleValueColumnName = "ruleValue";
    private const string RuleTypeColumnName = "ruleType";

    /// <summary>
    /// Gets or sets the unique identifier for the access rule.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_umbracoAccessRule", AutoIncrement = false)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the associated access entity for this access rule.
    /// </summary>
    [Column(AccessIdColumnName)]
    [ForeignKey(typeof(AccessDto), Name = "FK_umbracoAccessRule_umbracoAccess_id")]
    public Guid AccessId { get; set; }

    /// <summary>
    /// Gets or sets the specific value associated with the access rule, such as a user or group identifier.
    /// </summary>
    [Column(RuleValueColumnName)]
    [Index(IndexTypes.UniqueNonClustered, ForColumns = $"{RuleValueColumnName},{RuleTypeColumnName},{AccessIdColumnName}", Name = "IX_umbracoAccessRule")]
    public string? RuleValue { get; set; }

    /// <summary>
    /// Gets or sets the string value representing the type of the access rule.
    /// </summary>
    [Column(RuleTypeColumnName)]
    public string? RuleType { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the access rule was created.
    /// </summary>
    [Column("createDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the access rule was last updated.
    /// </summary>
    [Column("updateDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime UpdateDate { get; set; }
}
