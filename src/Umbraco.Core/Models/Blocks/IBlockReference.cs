namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Represents a data item reference for a Block Editor implementation.
/// </summary>
/// <remarks>
///     See:
///     https://github.com/umbraco/rfcs/blob/907f3758cf59a7b6781296a60d57d537b3b60b8c/cms/0011-block-data-structure.md#strongly-typed
/// </remarks>
public interface IBlockReference
{
    /// <summary>
    ///     Gets the content UDI.
    /// </summary>
    /// <value>
    ///     The content UDI.
    /// </value>
    Udi ContentUdi { get; }
}

/// <summary>
///     Represents a data item reference with settings for a Block editor implementation.
/// </summary>
/// <typeparam name="TSettings">The type of the settings.</typeparam>
/// <remarks>
///     See:
///     https://github.com/umbraco/rfcs/blob/907f3758cf59a7b6781296a60d57d537b3b60b8c/cms/0011-block-data-structure.md#strongly-typed
/// </remarks>
public interface IBlockReference<TSettings> : IBlockReference
{
    /// <summary>
    ///     Gets the settings.
    /// </summary>
    /// <value>
    ///     The settings.
    /// </value>
    TSettings Settings { get; }
}
