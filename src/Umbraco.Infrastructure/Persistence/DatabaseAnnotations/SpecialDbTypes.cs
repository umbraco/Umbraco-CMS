namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

/// <summary>
///     Known special DB types required for Umbraco.
/// </summary>
public enum SpecialDbTypes
{
    [Obsolete("Use NVARCHARMAX instead")]
    NTEXT,
    NCHAR,
    NVARCHARMAX,
}
