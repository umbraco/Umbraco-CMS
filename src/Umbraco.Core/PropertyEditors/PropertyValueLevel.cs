namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Indicates the level of a value.
/// </summary>
public enum PropertyValueLevel
{
    /// <summary>
    ///     The source value, i.e. what is in the database.
    /// </summary>
    Source,

    /// <summary>
    ///     The conversion intermediate value.
    /// </summary>
    Inter,

    /// <summary>
    ///     The converted value.
    /// </summary>
    Object,
}
