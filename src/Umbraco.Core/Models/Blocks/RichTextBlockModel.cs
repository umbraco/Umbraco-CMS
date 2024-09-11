// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     The strongly typed model for blocks in the Rich Text editor.
/// </summary>
[DataContract(Name = "richTextEditorBlocks", Namespace = "")]
public class RichTextBlockModel : BlockModelCollection<RichTextBlockItem>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RichTextBlockModel" /> class.
    /// </summary>
    /// <param name="list">The list to wrap.</param>
    public RichTextBlockModel(IList<RichTextBlockItem> list)
        : base(list)
    {
    }

    /// <summary>
    ///     Prevents a default instance of the <see cref="RichTextBlockModel" /> class from being created.
    /// </summary>
    private RichTextBlockModel()
        : this(new List<RichTextBlockItem>())
    {
    }

    /// <summary>
    ///     Gets the empty <see cref="RichTextBlockModel" />.
    /// </summary>
    /// <value>
    ///     The empty <see cref="RichTextBlockModel" />.
    /// </value>
    public static RichTextBlockModel Empty { get; } = new();
}
