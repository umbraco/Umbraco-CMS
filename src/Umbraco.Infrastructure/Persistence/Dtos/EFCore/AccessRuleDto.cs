using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(AccessRuleDtoConfiguration))]
public class AccessRuleDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.AccessRule;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    /// <summary>
    /// Gets or sets the unique identifier for the access rule.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the associated access entity for this access rule.
    /// </summary>
    public Guid AccessId { get; set; }

    /// <summary>
    /// Gets or sets the specific value associated with the access rule, such as a user or group identifier.
    /// </summary>
    public string? RuleValue { get; set; }

    /// <summary>
    /// Gets or sets the string value representing the type of the access rule.
    /// </summary>
    public string? RuleType { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the access rule was created.
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the access rule was last updated.
    /// </summary>
    public DateTime UpdateDate { get; set; }

    /// <summary>
    /// Gets or sets the parent access entry that owns this rule.
    /// </summary>
    public AccessDto? Access { get; set; }
}
