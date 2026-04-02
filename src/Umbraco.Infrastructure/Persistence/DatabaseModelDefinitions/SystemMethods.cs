namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

/// <summary>
/// Defines options available for default values derived from database system methods.
/// </summary>
public enum SystemMethods
{
    /// <summary>
    /// Represents a default value that is a generated GUID.
    /// </summary>
    NewGuid,

    /// <summary>
    /// Represents a default value that is the current date time.
    /// </summary>
    CurrentDateTime,

    /// <summary>
    /// Represents a default value that is the current UTC date time.
    /// </summary>
    CurrentUTCDateTime,
}
