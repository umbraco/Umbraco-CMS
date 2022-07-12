namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

/// <summary>
///     Attribute that represents the Null-setting of a column
/// </summary>
/// <remarks>
///     This should only be used for Columns that can be Null.
///     By convention the Columns will be "NOT NULL".
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class NullSettingAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the <see cref="NullSettings" /> for a column
    /// </summary>
    public NullSettings NullSetting { get; set; }
}
