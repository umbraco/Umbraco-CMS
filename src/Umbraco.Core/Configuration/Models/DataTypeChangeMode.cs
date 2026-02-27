namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Specifies the mode for allowing data type changes after they have been used.
/// </summary>
public enum DataTypeChangeMode
{
    /// <summary>
    ///     Data types can be changed after being used.
    /// </summary>
    True,

    /// <summary>
    ///     Data types cannot be changed after being used.
    /// </summary>
    False,

    /// <summary>
    ///     Data types cannot be changed after being used, and help text is displayed to explain this.
    /// </summary>
    FalseWithHelpText
}
