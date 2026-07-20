namespace Umbraco.Cms.Core.Persistence;

/// <summary>
///     Specifies the type of text column in the database.
/// </summary>
public enum TextColumnType
{
    /// <summary>
    ///     A variable-length Unicode string column (nvarchar).
    /// </summary>
    NVarchar,

    /// <summary>
    ///     A large Unicode text column (ntext).
    /// </summary>
    NText,
}
