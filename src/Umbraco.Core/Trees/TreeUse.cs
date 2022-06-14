namespace Umbraco.Cms.Core.Trees;

/// <summary>
///     Defines tree uses.
/// </summary>
[Flags]
public enum TreeUse
{
    /// <summary>
    ///     The tree is not used.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The tree is used as a main (section) tree.
    /// </summary>
    Main = 1,

    /// <summary>
    ///     The tree is used as a dialog.
    /// </summary>
    Dialog = 2,
}
