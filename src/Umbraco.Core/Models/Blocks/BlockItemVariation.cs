namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Represents a block item variation for culture.
/// </summary>
public class BlockItemVariation
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockItemVariation" /> class.
    /// </summary>
    public BlockItemVariation()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockItemVariation" /> class.
    /// </summary>
    /// <param name="contentKey">The content key.</param>
    /// <param name="culture">The culture.</param>
    public BlockItemVariation(Guid contentKey, string? culture)
    {
        ContentKey = contentKey;
        Culture = culture;
    }

    /// <summary>
    ///     Gets or sets the content key.
    /// </summary>
    /// <value>
    ///     The content key.
    /// </value>
    public Guid ContentKey { get; set; }

    /// <summary>
    ///     Gets or sets the culture.
    /// </summary>
    /// <value>
    ///     The culture.
    /// </value>
    public string? Culture { get; set; }
}
