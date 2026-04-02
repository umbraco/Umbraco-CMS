namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Represents a block item variation for culture and segment.
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
    /// <param name="segment">The segment.</param>
    public BlockItemVariation(Guid contentKey, string? culture, string? segment)
    {
        ContentKey = contentKey;
        Culture = culture;
        Segment = segment;
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

    /// <summary>
    ///     Gets or sets the segment.
    /// </summary>
    /// <value>
    ///     The segment.
    /// </value>
    public string? Segment { get; set; }
}
