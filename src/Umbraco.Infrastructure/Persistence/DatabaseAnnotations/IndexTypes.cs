namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

/// <summary>
///     Enum for the 3 types of indexes that can be created
/// </summary>
public enum IndexTypes
{
    Clustered,
    NonClustered,
    UniqueNonClustered,
}
